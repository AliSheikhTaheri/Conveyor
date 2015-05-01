namespace AST.ContentConveyor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using Umbraco.Core.Models;

    public class ExportContent : BaseContentManagement
    {
        #region Methods

        public XDocument SerializeToXml(string ids)
        {
            if (ids.Length > 0)
            {
                var nodes = ids.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                var xdocument = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));

                var xml = new XElement("Root");

                var dependentNodes = new Dictionary<int, ObjectTypes>();

                foreach (var node in nodes)
                {
                    var id = int.Parse(node);

                    var content = Services.ContentService.GetById(id);

                    var currentContentTag = SerializeContent(content, dependentNodes);

                    xml.Add(currentContentTag);
                }

                var firstLevelDependentNodes = dependentNodes.Where(x => !nodes.Contains(x.Key.ToString())).ToList();
                var secondLevelDependentNodes = new Dictionary<int, ObjectTypes>();

                foreach (var node in firstLevelDependentNodes)
                {
                    if (node.Value == ObjectTypes.Document)
                    {
                        xml.Add(SerializeContent(Services.ContentService.GetById(node.Key), secondLevelDependentNodes));
                    }
                    else if (node.Value == ObjectTypes.Media)
                    {
                        var media = Services.MediaService.GetById(node.Key);

                        do
                        {
                            xml.Add(SerializeMedia(media, secondLevelDependentNodes));
                            media = media.Parent();
                        }
                        while (media != null);
                    }
                }

                var secondLevelCollection = secondLevelDependentNodes.Where(x => !firstLevelDependentNodes.Select(f => f.Key).Contains(x.Key));

                foreach (var node in secondLevelCollection)
                {
                    if (node.Value == ObjectTypes.Document)
                    {
                        xml.Add(SerializeContent(Services.ContentService.GetById(node.Key)));
                    }
                    else if (node.Value == ObjectTypes.Media)
                    {
                        var media = Services.MediaService.GetById(node.Key);

                        do
                        {
                            xml.Add(SerializeMedia(media));
                            media = media.Parent();
                        }
                        while (media != null);
                    }
                }

                xdocument.Add(xml);

                return xdocument;
            }

            return null;
        }

        public IEnumerable<string> GetListOfAssets(XDocument xdoc)
        {
            return xdoc.Descendants()
                .Where(x => x.Attribute("dependentAsset") != null && !string.IsNullOrWhiteSpace(x.Attribute("dependentAsset").Value))
                .Select(x => x.Attribute("dependentAsset").Value);
        }

        #endregion
    }
}