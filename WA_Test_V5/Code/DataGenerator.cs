using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
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
        }

        private List<TreeViewElements> TransformToTree()
        {
            if (_initData == null)
                throw new NullReferenceException(nameof(_initData));
            //var set = new HashSet<Node>();
            int indexer = 1;
            //for (int i = 0; i < _initData.Count; i++)
            //{
            //    InnerTransform(set, null, _initData[i], 0, ref indexer);
            //}
            var rootNode = new TreeNode
            {
                Name = "Портфель проектов",
                ID = "0",
                CID = -2
            };
            //рекурсивное формирование дерева
            for (int i = 0; i < _initData.Count; i++)
            {
                InnerTransformTree(rootNode, _initData[i], 0, ref indexer);
            }

            //преобразование дерева в плоский массив
            var expandTree = new List<TreeViewElements>(_initData.Count * 10);

            ExpandTree(expandTree, rootNode);
            expandTree.RemoveAt(0);
            return expandTree;
        }

        private void InnerTransformTree(TreeNode parent, InitialDataModel initRow, int numCol, ref int indexer)
        {
            //случай, когда строка повторяется
            if (numCol == initRow.CellsData.Count - 1)
                return;

            var node = new TreeNode
            {
                Name = initRow.CellsData[numCol],
                ID = indexer.ToString(),
                CID = -2,
                Parent_ID = parent.ID
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

        private void ExpandTree(List<TreeViewElements> result, TreeNode node)
        {
            result.Add(node);
            if (node.Childs.Any())
            {
                foreach (var child in node.Childs)
                {
                    ExpandTree(result, child);
                }
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

            public HashSet<TreeNode> Childs { get; set; }
                = new HashSet<TreeNode>();

            public override bool Equals(object obj)
            {
                var node = obj as TreeNode;
                if (node == null)
                    return false;
                return Name == node.Name;
            }

            public override int GetHashCode()
            {
                var hashCode = -1919740922;
                //hashCode = hashCode * -1521134295 + Id.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
                return hashCode;
            }
        }

        class Node : TreeViewElements
        {
            public string UniqueName { get; set; }
            public override bool Equals(object obj)
            {
                var node = obj as Node;
                if (node == null)
                    return false;
                return UniqueName == node.UniqueName;
            }

            public override int GetHashCode()
            {
                var hashCode = -1919740922;
                //hashCode = hashCode * -1521134295 + Id.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(UniqueName);
                return hashCode;
            }
        }

    }
}