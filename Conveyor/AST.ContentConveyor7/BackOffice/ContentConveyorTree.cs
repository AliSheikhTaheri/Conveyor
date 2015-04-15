namespace AST.ContentConveyor7.BackOffice
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using umbraco.businesslogic;
    using umbraco.BusinessLogic.Actions;
    using umbraco.cms.presentation.Trees;
    using umbraco.interfaces;

    [Tree(Constants.ApplicationAlias, Constants.TreeAlias, Constants.TreeName, iconClosed: "icon-doc")]
    public class ContentConveyorTree : BaseTree
    {
        public ContentConveyorTree(string application)
            : base(application)
        {
        }

        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {
            rootNode.NodeType = "ContentConveyor";
            rootNode.NodeID = "init";
            rootNode.Menu = new List<IAction> { ActionRefresh.Instance };
        }

        public override void Render(ref XmlTree tree)
        {
            CreateNode(ref tree, "Import/Export", "Index");
        }

        public override void RenderJS(ref StringBuilder javascript)
        {
            javascript.Append(
                @"
                function openDashboard(action) {
                    document.getElementById('contentwrapper').scrolling = 'auto';
                    UmbClientMgr.contentFrame('../umbraco/BackOffice/" + Constants.ApplicationAlias + @"/' + action);
                }");
        }

        private void CreateNode(ref XmlTree tree, string name, string action)
        {
            var node = XmlTreeNode.Create(this);
            node.NodeID = 1.ToString();
            node.NodeType = "ContentConveyor";
            node.Text = name;
            node.Action = "javascript:openDashboard('" + action + "');";
            node.Icon = "icon-box";
            node.OpenIcon = "icon-box";
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