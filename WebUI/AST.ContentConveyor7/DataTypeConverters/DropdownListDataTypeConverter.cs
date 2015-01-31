namespace AST.ContentConveyor7.DataTypeConverters
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;

    using AST.ContentConveyor7;
    using AST.ContentConveyor7.Enums;
    using Umbraco.Core.Models;
    using Umbraco.Web.Mvc;

    public class DropdownListDataTypeConverter : BaseContentManagement, IDataTypeConverter
    {
        public void Export(Property property, XElement propertyTag, Dictionary<int, ObjectTypes> dependantNodes)
        {
            if (property.Value != null && !string.IsNullOrWhiteSpace(property.Value.ToString()))
            {
                int id;

                if (int.TryParse(property.Value.ToString(), out id))
                {
                    propertyTag.Value = Services.DataTypeService.GetPreValueAsString(id);
                }
            }
        }

        public string Import(XElement propertyTag)
        {
            var result = string.Empty;
            if (string.IsNullOrWhiteSpace(propertyTag.Value))
            {
                return result;
            }

            var dataTypeName = propertyTag.Attribute("dataTypeName").Value;
            var allDataTypeDefinitions = Services.DataTypeService.GetAllDataTypeDefinitions();
            var dataTypeDefinition = allDataTypeDefinitions.FirstOrDefault(x => x.Name == dataTypeName);

            if (dataTypeDefinition != null)
            {
                var collection = Services.DataTypeService.GetPreValuesCollectionByDataTypeId(dataTypeDefinition.Id);
                foreach (var preValue in collection.PreValuesAsDictionary)
                {
                    var value = preValue.Value.Value;
                    var id = preValue.Value.Id;
                    if (string.Equals(value, propertyTag.Value, StringComparison.CurrentCultureIgnoreCase))
                    {
                        result = id.ToString(CultureInfo.InvariantCulture);
                        break;
                    }
                }
            }
            else
            {
                result = propertyTag.Value;
            }

            return result;
        }
    }
}