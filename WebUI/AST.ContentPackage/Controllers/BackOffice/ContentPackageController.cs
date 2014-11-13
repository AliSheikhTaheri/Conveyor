namespace AST.ContentPackage.Controllers.BackOffice
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Mvc;
    using Ionic.Zip;
    using Umbraco.Core.Models;
    using Umbraco.Web.Mvc;

    public class ContentPackageController : UmbracoAuthorizedController
    {
        private const string ViewsFolder = "~/App_Plugins/BackOffice/ContentPackage/Views/{0}.cshtml";

        #region Actions

        public ActionResult Index()
        {
            return View(string.Format(ViewsFolder, "Index"));
        }

        public ActionResult ExportContent(string ids, string fileName = "")
        {
            var view = string.Format(ViewsFolder, "Index");

            if (string.IsNullOrEmpty(ids))
            {
                ModelState.AddModelError("exportError", "please select at least one node to export");
                return View(view);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    fileName = string.IsNullOrWhiteSpace(fileName) ? "ExportedContent" : fileName;

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
                        return File(ms, "application/zip", fileName);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("exportError", ex.Message);
                }
            }

            return View(view);
        }

        public ActionResult ImportContent(HttpPostedFileBase file, PublishTypes publishTypes)
        {
            var view = string.Format(ViewsFolder, "Index");

            if (file == null)
            {
                ModelState.AddModelError("importError", "Select a file to import");
            }

            if (ModelState.IsValid)
            {
                //try
                //{
                    var ic = new ImportContent { PublishType = publishTypes };

                    ic.Import(file);

                    view = string.Format(ViewsFolder, "ImportReport");

                    return View(view, ic.Report);
                //}
                //catch (Exception ex)
                //{
                //    ModelState.AddModelError("importError", ex.Message);
                //}
            }

            return View(view);
        }

        public ActionResult CheckCompatibility()
        {
            var dts = Services.DataTypeService;

            var allDataType = dts.GetAllDataTypes();

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
                    children = new List<Node>(),
                };

                var contentAtRoot = Services.ContentService.GetRootContent();

                foreach (var n in contentAtRoot)
                {
                    node.children.Add(GenerateJsonForTree(n));
                }

                return Json(node, JsonRequestBehavior.AllowGet);
            }

            var children = Services.ContentService.GetChildren(id);

            var nodes = children.Select(GenerateJsonForTree).ToList();

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