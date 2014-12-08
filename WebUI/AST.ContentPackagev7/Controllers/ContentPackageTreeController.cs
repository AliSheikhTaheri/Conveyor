namespace AST.ContentPackagev7.Controllers
{
    using System.Net.Http.Formatting;

    using Umbraco.Web.Models.Trees;
    using Umbraco.Web.Mvc;
    using Umbraco.Web.Trees;

    [PluginController("CustomSection")]
    [Tree("ContentPackage", "ContentPackageTree", "AST Content Package", iconClosed: "icon-doc")]
    public class ContentPackageTreeController : TreeController
    {
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            throw new System.NotImplementedException();
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            throw new System.NotImplementedException();
        }
    }
}
