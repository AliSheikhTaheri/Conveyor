using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Umbraco.Core.Models;

namespace AST.ContentConveyor.DataTypeConverters
{
    public class MediaPickerDataTypeConverter : BaseContentManagement, IDataTypeConverter
    {
        public void Export(Property property, XElement propertyTag, Dictionary<int, ObjectTypes> dependantNodes)
        {
            if (property.Value != null && !string.IsNullOrWhiteSpace(property.Value.ToString()))
            {
                var id = int.Parse(property.Value.ToString());
                var media = Services.MediaService.GetById(id);

                if (media == null) return;

                propertyTag.Value = media.Key.ToString();

                if (!dependantNodes.ContainsKey(media.Id))
                {
                    dependantNodes.Add(media.Id, ObjectTypes.Media);
                }
            }
        }

        public string Import(XElement propertyTag)
        {
            var result = string.Empty;

            if (!string.IsNullOrWhiteSpace(propertyTag.Value))
            {
                var guid = new Guid(propertyTag.Value);
                var id = Services.MediaService.GetById(guid).Id;
                result = id.ToString();
            }

            return result;
        }
    }
}