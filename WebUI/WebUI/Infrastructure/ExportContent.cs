namespace WebUI.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Linq;
    using Umbraco.Core;
    using Umbraco.Core.Models;
    using Umbraco.Core.Services;

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

                var dependentNodes = new Dictionary<Guid, UmbracoObjectTypes>();

                foreach (var node in nodes)
                {
                    var id = int.Parse(node);

                    var content = Services.ContentService.GetById(id);

                    var currentContentTag = SerialiseContent(content, dependentNodes);

                    xml.Add(currentContentTag);
                }

                foreach (var node in dependentNodes)
                {
                    if (node.Value == UmbracoObjectTypes.Document)
                    {
                        xml.Add(SerialiseContent(Services.ContentService.GetById(node.Key)));
                    }
                    else if (node.Value == UmbracoObjectTypes.Media)
                    {
                        xml.Add(SerialiseMedia(Services.MediaService.GetById(node.Key)));
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