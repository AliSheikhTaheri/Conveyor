﻿namespace WebUI.Controllers.BackOffice
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
                return View("~/App_Plugins/BackOffice/ContentPackage/Views/Index.cshtml");
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

            return View("~/App_Plugins/BackOffice/ContentPackage/Views/Index.cshtml");
        }

        #endregion

        #region Ajax Actions

        public JsonResult ContentTreeAsJsonResult()
        {
            var ser = new JavaScriptSerializer();

            var node = new Node
            {
                title = "Content",
                active = true,
                key = "-1",
                folder = true,
                hideCheckbox = true,
                expanded = true,
            };

            var nodes = new[] { node };

            node.children = new List<Node>();

            foreach (var n in Services.ContentService.GetRootContent())
            {
                node.children.Add(GenerateJsonForTree(n.Id));
            }

            var json = ser.Serialize(nodes);

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Helpers

        private Node GenerateJsonForTree(int id)
        {
            var currentNode = Services.ContentService.GetById(id);

            var temp = new Node
            {
                title = currentNode.Name,
                key = currentNode.Id.ToString(),
                folder = currentNode.Children().Any(),
            };

            if (currentNode.Children() != null && currentNode.Children().Any())
            {
                temp.children = new List<Node>();
                foreach (var c in currentNode.Children())
                {
                    var t = GenerateJsonForTree(c.Id);

                    temp.children.Add(t);
                }
            }

            return temp;
        }

        private class Node
        {
            public string title { get; set; }

            public string key { get; set; }

            public List<Node> children { get; set; }

            public bool folder { get; set; }

            public bool hideCheckbox { get; set; }

            public bool active { get; set; }

            public bool expanded { get; set; }
        }

        #endregion
    }
}