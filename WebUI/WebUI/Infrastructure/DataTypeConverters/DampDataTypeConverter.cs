using System.Linq;

namespace WebUI.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Web;
    using System.Xml.Linq;
    using DigibizTree;
    using Umbraco.Core.Models;
    using DampItem = DAMP.PropertyEditorValueConverter.DAMP_Item;
    using DampModel = DAMP.PropertyEditorValueConverter.Model;

    public class DampDataTypeConverter : BaseContentManagement, IDataTypeConverter
    {
        public void Export(Property property, XElement propertyTag, Dictionary<int, ObjectTypes> dependantNodes)
        {
            if (property.Value != null && !string.IsNullOrWhiteSpace(property.Value.ToString()))
            {
                var items = new DampModel(property.Value.ToString());
                var guidList = new List<Guid>();

                if (items.Any)
                {
                    foreach (var item in items)
                    {
                        guidList.Add(Services.MediaService.GetById(item.Id).Key);

                        if (!dependantNodes.ContainsKey(item.Id))
                        {
                            dependantNodes.Add(item.Id, ObjectTypes.Media);
                        }
                    }
                }

                propertyTag.Value = string.Join(",", guidList);
            }
        }

        public string Import(XElement propertyTag)
        {
            var result = string.Empty;

            if (!string.IsNullOrWhiteSpace(propertyTag.Value))
            {
                var listOfGuid = propertyTag.Value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                var listOfIds = new List<string>();

                foreach (var guid in listOfGuid)
                {
                    var media = Services.MediaService.GetById(new Guid(guid));

                    if (media != null)
                    {
                        listOfIds.Add(media.Id.ToString());
                    }
                }

                if (listOfIds.Any())
                {
                    var xml = DigibizMediaHelper.GetXML(listOfIds.ToArray());
                    result = HttpUtility.HtmlDecode(xml.ToString());
                }
            }

            return result;
        }
    }
}