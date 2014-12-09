namespace AST.ContentPackagev7.Controllers
{
    using System.Net.Http.Formatting;

    using Umbraco.Web.Models.Trees;
    using Umbraco.Web.Mvc;
    using Umbraco.Web.Trees;

    [PluginController("ContentPackage")]
    [Tree("ContentPackage", "ContentPackageTree", "AST Content Package", iconClosed: "icon-doc")]
    public class ContentPackageTreeController : TreeController
    {
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var tree = new TreeNodeCollection
            {
                CreateTreeNode("1", id, queryStrings, "Donations", "icon-donate", false)
            };

            return tree;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            return new MenuItemCollection();
        }
    }
}
