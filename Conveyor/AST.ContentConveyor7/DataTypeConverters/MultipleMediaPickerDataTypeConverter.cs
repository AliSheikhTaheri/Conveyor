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

    /// <summary>
    /// Type Converter for the MultipleMediaPicker editor (u7)
    /// Converts a list of one or several media ids into Guids (Export) or from Guids (Import) 
    /// </summary>
    public class MultipleMediaPickerDataTypeConverter : BaseContentManagement, IDataTypeConverter
    {
        /// <summary>
        /// Export - generic IDataTypeConverter interface
        /// </summary>
        public void Export(Property property, XElement propertyTag, Dictionary<int, ObjectTypes> dependantNodes)
        {
            string value = (property.Value ?? "").ToString();
            propertyTag.Value = ExportValue(value, dependantNodes);
        }

        /// <summary>
        /// Export - Inner method available to other Type Converters
        /// </summary>
        public string ExportValue(string value, Dictionary<int, ObjectTypes> dependantNodes)
        {
            if (!string.IsNullOrWhiteSpace(value.ToString()))
            {
                var ids = value.ToString().Split(',').Select(s => int.Parse(s));
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
                return String.Join(",", guids);
            }
            return value;
        }

        /// <summary>
        /// Import - generic IDataTypeConverter interface
        /// </summary>
        public string Import(XElement propertyTag)
        {
            return ImportValue(propertyTag.Value);
        }

        /// <summary>
        /// Import - Inner method available to other Type Converters
        /// </summary>
        public string ImportValue(string value)
        {
            var result = string.Empty;

            if (!string.IsNullOrWhiteSpace(value))
            {
                var guids = value.Split(',').Select(s => new Guid(s));
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