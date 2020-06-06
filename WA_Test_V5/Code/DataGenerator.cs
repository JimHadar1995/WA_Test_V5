using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WA_Test_V5.Interface.TreeView;
using WA_Test_V5.Models;

namespace WA_Test_V5.Code
{

    public sealed class DataGenerator
    {
        private readonly string _filePath;

        private List<InitialDataModel> _initData;

        public DataGenerator(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (!File.Exists(filePath))
                throw new FileNotFoundException();
            _filePath = filePath;
        }


        public List<TreeViewElements> GetTree()
        {
            Init();
            return TransformToTree();
        }

        /// <summary>
        /// Чтение данных из файла
        /// </summary>
        private void Init()
        {
            var fileInfo = new FileInfo(_filePath);
            var pack = new ExcelPackage(fileInfo);
            var sheets = pack.Workbook.Worksheets;
            var dataSheet = sheets.First();
            var numberOfRows = dataSheet.Dimension.End.Row;
            _initData = new List<InitialDataModel>(numberOfRows);
            ExcelRange cells = dataSheet.Cells;
            for (int rowI = 2; rowI <= numberOfRows; rowI++)
            {
                _initData.Add(GetInitialRowData(cells, rowI));
            }
            //Убираем дублирующиеся строки
            _initData = _initData.Distinct(new InitialDataModelEqualityComparer()).ToList();
            //Сортируем, исползуя сравнение строк по умолчанию
            _initData.Sort((x, y) => Comparer<string>.Default.Compare(x.Uniq, y.Uniq));            
        }

        private List<TreeViewElements> TransformToTree()
        {
            if (_initData == null)
                throw new NullReferenceException(nameof(_initData));
            int indexer = 1;
            var rootNode = new TreeNode
            {
                Name = "Портфель проектов",
                ID = "0",
                CID = 0,
                Parent = null
            };
            //рекурсивное формирование дерева
            for (int i = 0; i < _initData.Count; i++)
            {
                InnerTransformTree(rootNode, _initData[i], 0, ref indexer);
            }

            //преобразование дерева в плоский массив
            var expandTree = new List<TreeNode>(indexer);

            ExpandTree(expandTree, rootNode);
            expandTree.RemoveAt(0);
            return expandTree.Cast<TreeViewElements>().ToList();
        }

        private void InnerTransformTree(TreeNode parent, InitialDataModel initRow, int numCol, ref int indexer)
        {
            TreeNode node;
            //случай, когда строка повторяется, но не совпадает CID
            if (numCol == initRow.CellsData.Count - 1)
            {
                node = new TreeNode
                {
                    Name = initRow.CellsData[numCol - 1],
                    ID = indexer.ToString(),
                    CID = int.Parse(initRow.CellsData[numCol]),
                    Parent_ID = parent.Parent_ID,
                    Parent = parent.Parent,
                    UniqueName = string.Join(".", initRow.CellsData.GetRange(0, initRow.CellsData.Count)),
                    Old_Cid = initRow.CellsData.Last()
                };
                indexer++;
                parent.Parent.Childs.Add(node);
                return;
            }
            node = new TreeNode
            {
                Name = initRow.CellsData[numCol],
                ID = indexer.ToString(),
                CID = -2,
                Parent_ID = parent.ID,
                Parent = parent,
                UniqueName = string.Join(".", initRow.CellsData.GetRange(0, numCol + 1)),
                Old_Cid = initRow.CellsData.Last()
            };

            numCol++;
            if (parent.Childs.TryGetValue(node, out var actualNode))
            {
                InnerTransformTree(actualNode, initRow, numCol, ref indexer);
                return;
            }
            indexer++;
            parent.Childs.Add(node);

            if (numCol < initRow.CellsData.Count - 1)
            {
                InnerTransformTree(node, initRow, numCol, ref indexer);                
            }
            else
            {
                //присваиваем Cid
                node.CID = int.Parse(initRow.CellsData.Last());
            }
        }

        private void ExpandTree(List<TreeNode> result, TreeNode node)
        {
            result.Add(node);
            if (node.Childs.Any())
            {
                foreach (var child in node.Childs)
                {
                    ExpandTree(result, child);
                }
                node.Childs.Clear();
                node.Parent = null;
            }
        }

        private InitialDataModel GetInitialRowData(ExcelRange cells, int rowI)
        {
            var rowData = new InitialDataModel();

            for (int i = 1; i <= 10; i++)
            {
                rowData.CellsData.Add(cells[rowI, i].Value.ToString());
            }
            return rowData;
        }

        class TreeNode : TreeViewElements
        {
            public string UniqueName { get; set; }
            public string Old_Cid { get; set; }
            public TreeNode Parent { get; set; }
            public HashSet<TreeNode> Childs { get; set; }
                = new HashSet<TreeNode>();

            public override bool Equals(object obj)
            {
                var node = obj as TreeNode;
                if (node == null)
                    return false;
                return UniqueName == node.UniqueName;
            }

            public override int GetHashCode()
            {
                var hashCode = -1919740922;
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(UniqueName);
                return hashCode;
            }
        }

    }
}