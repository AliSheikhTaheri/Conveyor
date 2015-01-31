namespace AST.ContentConveyor7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    using AST.ContentConveyor7.Enums;

    using Umbraco.Core.Models;

    public class ExportContent : BaseContentManagement
    {
        #region Methods

        public XDocument SerialiseToXml(string ids)
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

                    var currentContentTag = SerialiseContent(content, dependentNodes);

                    xml.Add(currentContentTag);
                }

                var firstLevelDependentNodes = dependentNodes.Where(x => !nodes.Contains(x.Key.ToString())).ToList();
                var secondLevelDependentNodes = new Dictionary<int, ObjectTypes>();

                foreach (var node in firstLevelDependentNodes)
                {
                    if (node.Value == ObjectTypes.Document)
                    {
                        xml.Add(SerialiseContent(Services.ContentService.GetById(node.Key), secondLevelDependentNodes));
                    }
                    else if (node.Value == ObjectTypes.Media)
                    {
                        var media = Services.MediaService.GetById(node.Key);

                        do
                        {
                            xml.Add(SerialiseMedia(media, secondLevelDependentNodes));
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
                        xml.Add(SerialiseContent(Services.ContentService.GetById(node.Key)));
                    }
                    else if (node.Value == ObjectTypes.Media)
                    {
                        var media = Services.MediaService.GetById(node.Key);

                        do
                        {
                            xml.Add(SerialiseMedia(media));
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
                .Where(x => x.Attribute("umbracoFile") != null && !string.IsNullOrWhiteSpace(x.Attribute("umbracoFile").Value))
                .Select(x => x.Attribute("umbracoFile").Value);
        }

        #endregion
    }
}