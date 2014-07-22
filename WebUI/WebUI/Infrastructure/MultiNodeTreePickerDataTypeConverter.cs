namespace WebUI.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using Ionic.Zip;
    using umbraco;
    using Umbraco.Core;
    using Umbraco.Core.Models;
    using Umbraco.Core.Services;

    public class MultiNodeTreePickerDataTypeConverter : BaseContentManagement, IDataTypeConverter
    {
        public void Export(Property property, XElement propertyTag, Dictionary<Guid, UmbracoObjectTypes> dependantNodes)
        {
            if (property.Value != null && !string.IsNullOrWhiteSpace(property.Value.ToString()))
            {
                int[] nodeIds;

                if (XmlHelper.CouldItBeXml(property.Value.ToString()))
                {
                    nodeIds = uQuery.GetXmlIds(property.Value.ToString());
                }
                else
                {
                    nodeIds = property.Value.ToString().Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
                }

                if (nodeIds.Length > 0)
                {
                    var nodeType = uQuery.GetUmbracoObjectType(nodeIds[0]);

                    propertyTag.Add(new XAttribute("umbracoObjectType", nodeType));

                    var guidList = new List<Guid>();

                    foreach (var id in nodeIds)
                    {
                        if (nodeType == uQuery.UmbracoObjectType.Document)
                        {
                            var guid = Services.ContentService.GetById(id).Key;

                            guidList.Add(guid);
                            if (!dependantNodes.ContainsKey(guid))
                            {
                                dependantNodes.Add(guid, UmbracoObjectTypes.Document);
                            }
                        }
                        else if (nodeType == uQuery.UmbracoObjectType.Media)
                        {
                            var guid = Services.MediaService.GetById(id).Key;
                            guidList.Add(guid);
                            
                            if (!dependantNodes.ContainsKey(guid))
                            {
                                dependantNodes.Add(guid, UmbracoObjectTypes.Media);
                            }
                        }
                    }

                    propertyTag.Value = string.Join(",", guidList);
                }
            }
        }

        public string Import(XElement propertyTag)
        {
            return string.Empty;
        }
    }
}