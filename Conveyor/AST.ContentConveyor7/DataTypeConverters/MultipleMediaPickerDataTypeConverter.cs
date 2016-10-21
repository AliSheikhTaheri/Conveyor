namespace AST.ContentConveyor7.DataTypeConverters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    using AST.ContentConveyor7;
    using AST.ContentConveyor7.Enums;
    using AST.ContentConveyor7.Utilities;

    using Umbraco.Core.Models;

    public class MultipleMediaPickerDataTypeConverter : BaseContentManagement, IDataTypeConverter
    {
        public void Export(Property property, XElement propertyTag, Dictionary<int, ObjectTypes> dependantNodes)
        {
            if (property.Value != null && !string.IsNullOrWhiteSpace(property.Value.ToString()))
            {
                var ids = property.Value.ToString().Split(',').Select(s => int.Parse(s));
                List<string> guids = new List<string>(ids.Count());

                foreach (int id in ids)
                {
                    var media = Services.MediaService.GetById(id);
                    if (media != null)
                    {
                        guids.Add(media.Key.ToString());
                        if (!dependantNodes.ContainsKey(media.Id))
                        {
                            dependantNodes.Add(media.Id, ObjectTypes.Media);
                        }
                    }
                }
                propertyTag.Value = String.Join(",", guids);
            }
        }

        public string Import(XElement propertyTag)
        {
            var result = string.Empty;

            if (!string.IsNullOrWhiteSpace(propertyTag.Value))
            {
                var guids = propertyTag.Value.Split(',').Select(s => new Guid(s));
                List<string> ids = new List<string>(guids.Count());

                foreach (Guid guid in guids)
                {
                    var media = Services.MediaService.GetById(guid);
                    if (media != null)
                    {
                        ids.Add(media.Id.ToString());
                    }
                }
                result = String.Join(",", ids);

            }
            return result;
        }
    }
}