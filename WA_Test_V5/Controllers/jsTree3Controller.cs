using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WA_Test_V5.Code;
using WA_Test_V5.Interface.JsTreeNodes;

namespace jsTree3.Controllers
{
    public class jsTree3Controller : Controller
    {
        public ActionResult Demo()
        {
            return View();
        }

        public ActionResult AJAXDemo()
        {
            return View();
        }
        public JsonResult GetJsTree3Data()
        {
            var generator = new DataGenerator(Server.MapPath("~/Content/DataForEdit.xlsx"));
            var tree = generator.GetTree();
            int nodeUnicID = 0;
            var ParentsDic = new Dictionary<int, string>();//no
            var ReadyList = new List<JsTree3Node>();
            var mainNode = new JsTree3Node()
            {
                id = "0",
                text = "Портфель проектов",
                state = new State(true, false, false),
                children = new List<JsTree3Node>(),
                data = 0
            };
            ReadyList.Add(mainNode);
            ParentsDic.Add(nodeUnicID, "0");
            nodeUnicID++;
            foreach (var elem in tree)
            {
                var newNode = new JsTree3Node()
                {
                    id = elem.ID,
                    text = elem.Name,
                    state = new State(false, false, false),
                    children = new List<JsTree3Node>(),
                    data = elem.CID
                };
                if (ParentsDic.ContainsValue(elem.Parent_ID.ToString()))
                    ReadyList[ParentsDic.FirstOrDefault(x => x.Value == elem.Parent_ID.ToString()).Key].children.Add(newNode);

                ReadyList.Add(newNode);
                ParentsDic.Add(nodeUnicID, elem.ID.ToString());
                nodeUnicID++;
            }
            return Json(mainNode, JsonRequestBehavior.AllowGet);
        }
    }
}
