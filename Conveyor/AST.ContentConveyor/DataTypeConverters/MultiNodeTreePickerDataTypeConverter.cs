using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.Models;

namespace AST.ContentConveyor.DataTypeConverters
{
    public class MultiNodeTreePickerDataTypeConverter : BaseContentManagement, IDataTypeConverter
    {
        public void Export(Property property, XElement propertyTag, Dictionary<int, ObjectTypes> dependantNodes)
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
                    var nodeType = uQuery.GetUmbracoObjectType(nodeIds[0]).ToString();

                    propertyTag.Add(new XAttribute("objectType", nodeType));

                    var guidList = new List<Guid>();

                    foreach (var id in nodeIds)
                    {
                        if (nodeType == ObjectTypes.Document.ToString())
                        {
                            var guid = Services.ContentService.GetById(id).Key;

                            guidList.Add(guid);
                            if (!dependantNodes.ContainsKey(id))
                            {
                                dependantNodes.Add(id, ObjectTypes.Document);
                            }
                        }
                        else if (nodeType == ObjectTypes.Media.ToString())
                        {
                            var guid = Services.MediaService.GetById(id).Key;
                            guidList.Add(guid);

                            if (!dependantNodes.ContainsKey(id))
                            {
                                dependantNodes.Add(id, ObjectTypes.Media);
                            }
                        }
                    }

                    propertyTag.Value = string.Join(",", guidList);
                }
            }
        }

        public string Import(XElement propertyTag)
        {
            var result = string.Empty;

            if (!string.IsNullOrWhiteSpace(propertyTag.Value))
            {
                var listOfGuid = propertyTag.Value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                var objectType = propertyTag.Attribute("objectType").Value;

                var listOfIds = new List<int>();

                foreach (var guid in listOfGuid)
                {
                    if (objectType == ObjectTypes.Document.ToString())
                    {
                        var content = Services.ContentService.GetById(new Guid(guid));

                        if (content != null)
                        {
                            listOfIds.Add(content.Id);
                        }
                    }
                    else if (objectType == ObjectTypes.Media.ToString())
                    {
                        var media = Services.MediaService.GetById(new Guid(guid));
                        
                        if (media != null)
                        {
                            listOfIds.Add(media.Id);
                        }
                    }
                }

                if (listOfIds.Any())
                {
                    result = string.Join(",", listOfIds);
                }
            }

            return result;
        }
    }
}