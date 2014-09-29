namespace AST.ContentPackage.UrlPicker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using uComponents.DataTypes.UrlPicker;
    using uComponents.DataTypes.UrlPicker.Dto;
    using umbraco.cms.businesslogic.datatype;
    using Umbraco.Core.Models;

    public class UrlPickerDataTypeConverter : BaseContentManagement, IDataTypeConverter
    {
        public void Export(Property property, XElement propertyTag, Dictionary<int, ObjectTypes> dependantNodes)
        {
            if (property.Value != null && !string.IsNullOrWhiteSpace(property.Value.ToString()))
            {
                var ups = UrlPickerState.Deserialize(property.Value.ToString());

                if (ups != null)
                {
                    // find the data type definition
                    var dtd =
                    Services.DataTypeService.GetAllDataTypeDefinitions()
                        .FirstOrDefault(x => x.Name == propertyTag.Attribute("dataTypeName").Value);

                    // store how data is saved
                    if (dtd != null)
                    {
                        var values = PreValues.GetPreValues(dtd.Id);

                        if (values.Count >= 1)
                        {
                            var pv = values[1] as PreValue;

                            propertyTag.Add(new XAttribute("format", pv.Value));
                        }
                    }

                    propertyTag.Add(new XAttribute("mode", ups.Mode.ToString()));
                    propertyTag.Value = property.Value.ToString();

                    // find the id and convert it to guid
                    if (ups.NodeId != null && ups.NodeId.Value > 0)
                    {
                        if (ups.Mode == UrlPickerMode.Content)
                        {
                            var content = Services.ContentService.GetById(ups.NodeId.Value);

                            if (content != null)
                            {
                                propertyTag.Add(new XAttribute("nodeId", content.Key.ToString()));
                            }
                        }
                        else if (ups.Mode == UrlPickerMode.Media)
                        {
                            var media = Services.MediaService.GetById(ups.NodeId.Value);
                            if (media != null)
                            {
                                propertyTag.Add(new XAttribute("nodeId", media.Key.ToString()));
                            }
                        }
                    }
                }
            }
        }

        public string Import(XElement propertyTag)
        {
            var result = string.Empty;

            if (!string.IsNullOrWhiteSpace(propertyTag.Value))
            {
                var ups = UrlPickerState.Deserialize(propertyTag.Value);

                if (ups.Mode == UrlPickerMode.Content && propertyTag.Attribute("nodeId") != null)
                {
                    var content = Services.ContentService.GetById(new Guid(propertyTag.Attribute("nodeId").Value));
                    if (content != null)
                    {
                        ups.NodeId = content.Id;
                    }
                }
                else if (ups.Mode == UrlPickerMode.Media && propertyTag.Attribute("nodeId") != null)
                {
                    var media = Services.MediaService.GetById(new Guid(propertyTag.Attribute("nodeId").Value));
                    if (media != null)
                    {
                        ups.NodeId = media.Id;
                    }
                }

                UrlPickerDataFormat format;
                if (Enum.TryParse(propertyTag.Attribute("format").Value, out format))
                {
                    result = ups.Serialize(format);
                }
            }

            return result;
        }
    }
}
