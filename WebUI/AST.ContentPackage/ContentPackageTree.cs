namespace AST.ContentPackage
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using umbraco.businesslogic;
    using umbraco.BusinessLogic.Actions;
    using umbraco.cms.presentation.Trees;
    using umbraco.interfaces;

    [Tree(Constants.ApplicationAlias, Constants.TreeAlias, Constants.TreeName)]
    public class ContentPackageTree : BaseTree
    {
        public ContentPackageTree(string application)
            : base(application)
        {
        }

        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {
            rootNode.NodeType = "example";
            rootNode.NodeID = "init";
            rootNode.Menu = new List<IAction> { ActionRefresh.Instance };
        }

        public override void Render(ref XmlTree tree)
        {
            CreateReportNode(ref tree, Constants.ApplicationName, "Index");
        }

        public override void RenderJS(ref StringBuilder javascript)
        {
            javascript.Append(
                @"
                function openMemberReporting(report) {
                    document.getElementById('right').scrolling = 'auto';
                    UmbClientMgr.contentFrame('../App_Plugins/Backoffice/" + Constants.ApplicationAlias + @"/' + report);
                }");
        }

        private void CreateReportNode(ref XmlTree tree, string name, string action)
        {
            var node = XmlTreeNode.Create(this);
            node.NodeID = 1.ToString();
            node.NodeType = "report";
            node.Text = name;
            node.Action = "javascript:openMemberReporting('" + action + "');";
            node.Icon = "../../../App_Plugins/BackOffice/ContentPackage/Images/Icons/importDocumenttype.png";
            node.OpenIcon = "../../../App_Plugins/BackOffice/ContentPackage/Images/Icons/importDocumenttype.png";
            node.HasChildren = false;
            node.Menu = new List<IAction>();
            OnBeforeNodeRender(ref tree, ref node, EventArgs.Empty);
            if (node != null)
            {
                tree.Add(node);
                OnAfterNodeRender(ref tree, ref node, EventArgs.Empty);
            }
        }
    }
}