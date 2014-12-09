namespace AST.ContentPackagev7.Controllers
{
    using System.Web.Mvc;

    using AST.ContentPackagev7.Models;

    using Umbraco.Web.Mvc;

    public class ContentPackageController : UmbracoAuthorizedController
    {
        private const string ViewsFolder = "~/App_Plugins/BackOffice/ContentPackage/Views/{0}.cshtml";

        public ActionResult Index()
        {
            var model = new TestModel { StringOne = "first string", StringTwo = "second string" };
            return View(string.Format(ViewsFolder, "Index"), model);
        }

        public ActionResult ExportContent()
        {
            return View();
        }

        public ActionResult CheckCompatibility()
        {
            return View();
        }

        public ActionResult ImportContent()
        {
            return View();
        }
    }
}