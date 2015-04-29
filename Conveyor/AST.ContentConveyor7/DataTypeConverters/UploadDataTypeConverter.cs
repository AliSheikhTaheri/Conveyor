using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using AST.ContentConveyor7.Enums;
using AST.ContentConveyor7.Utilities;
using Umbraco.Core.Models;

namespace AST.ContentConveyor7.DataTypeConverters
{
    public class UploadDataTypeConverter : BaseContentManagement, IDataTypeConverter, IUploadDataTypeConverter
    {
        public void Export(Property property, XElement propertyTag, Dictionary<int, ObjectTypes> dependantNodes)
        {
            if (property.Value != null && !string.IsNullOrWhiteSpace(property.Value.ToString()))
            {
                var file = property.Value.ToString();

                // TODO for v6
                if (FileHelpers.FileExists(file))
                {
                    propertyTag.Add(
                    new XAttribute(property.Alias , file),
                    new XAttribute("fileName", file.Split('/').Last()),
                    new XAttribute("objectType", ObjectTypes.File),
                    new XAttribute("dependentAsset", file));            // special attribute used to retrieve dependent assets
                }
            }
        }

        public string Import(XElement propertyTag)
        {
            var result = string.Empty;

            if (propertyTag.Attribute("dependentAsset") != null)
            {
                result = propertyTag.Attribute("dependentAsset").Value;
            }

            return result;
        }

        public string GetUrl(string propertyData)
        {
            return propertyData;
        }
    }
}
