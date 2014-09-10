namespace WebUI.Controllers.BackOffice
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Mvc;
    using System.Web.Script.Serialization;
    using Infrastructure;
    using Ionic.Zip;
    using Umbraco.Core.Models;
    using Umbraco.Web.Mvc;

    public class ContentPackageController : UmbracoAuthorizedController
    {
        private const string ViewsFolder = "~/App_Plugins/BackOffice/ContentPackage/Views/{0}.cshtml";

        #region Actions

        public ActionResult Index()
        {
            return View("~/App_Plugins/BackOffice/ContentPackage/Views/Index.cshtml");
        }

        public ActionResult ExportContent(string ids)
        {
            if (string.IsNullOrEmpty(ids))
            {
                ModelState.AddModelError("error", "please select at least one node to export");
                return View(string.Format(ViewsFolder, "Index"));
            }

            var export = new ExportContent();

            var xdoc = export.SerialiseToXml(ids);
            var files = export.GetListOfAssets(xdoc);

            var ms = new MemoryStream();
            var msXml = new MemoryStream();

            using (var zip = new ZipFile())
            {
                foreach (var f in files.Distinct())
                {
                    zip.AddFile(HostingEnvironment.MapPath(f), f.Replace(f.Split('/').Last(), string.Empty));
                }

                xdoc.Save(msXml);
                msXml.Position = 0;
                zip.AddEntry(Constants.ContentFileName, msXml);
                zip.Save(ms);

                ms.Position = 0;
                return File(ms, "application/zip");
            }
        }

        public ActionResult ImportContent(HttpPostedFileBase file)
        {
            var ic = new ImportContent();

            ic.Import(file);

            return View(string.Format(ViewsFolder, "Index"));
        }

        public ActionResult CheckCompatibility()
        {
            var dts = Services.DataTypeService;

            var allDataType = dts.GetAllDataTypes();

            //var xx = IDataType

            var config = new Config();
            var compatibleDataTypes = config.GetSpecialDataTypes().Select(x => x.Key).ToList();
            compatibleDataTypes.AddRange(config.GetOtherDataTypes().Keys);

            var notCompatibleDataTypes = allDataType.Where(x => !compatibleDataTypes.Contains(x.Id));

            return View(string.Format(ViewsFolder, "CompatibilityCheck"), notCompatibleDataTypes);
        }

        #endregion

        #region Ajax Actions

        public JsonResult ContentTreeAsJsonResult(int id = 0)
        {
            if (id == 0)
            {
                var node = new Node
                {
                    title = "Content",
                    active = true,
                    key = "-1",
                    folder = true,
                    hideCheckbox = true,
                    expanded = true,
                };

                node.children = new List<Node>();

                var contentAtRoot = Services.ContentService.GetRootContent();

                foreach (var n in contentAtRoot)
                {
                    node.children.Add(GenerateJsonForTree(n));
                }

                return Json(node, JsonRequestBehavior.AllowGet);
            }

            var children = Services.ContentService.GetChildren(id);
            var nodes = new List<Node>();

            foreach (var n in children)
            {
                nodes.Add(GenerateJsonForTree(n));
            }


            return Json(nodes, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Helpers

        private Node GenerateJsonForTree(IContent content)
        {
            var isFolder = content.Children().Any();

            var temp = new Node
            {
                title = content.Name,
                key = content.Id.ToString(),
                folder = isFolder,
                lazy = isFolder
            };

            //if (currentNode.Children() != null && currentNode.Children().Any())
            //{
            //    temp.children = new List<Node>();
            //    foreach (var c in currentNode.Children())
            //    {
            //        var t = GenerateJsonForTree(c.Id);

            //        temp.children.Add(t);
            //    }
            //}

            return temp;
        }

        private class Node
        {
            public string title { get; set; }

            public string key { get; set; }

            public List<Node> children { get; set; }

            public bool folder { get; set; }

            public bool lazy { get; set; }

            public bool hideCheckbox { get; set; }

            public bool active { get; set; }

            public bool expanded { get; set; }
        }

        #endregion
    }
}