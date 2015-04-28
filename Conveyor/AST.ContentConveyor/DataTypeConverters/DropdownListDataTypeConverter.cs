using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using umbraco.cms.businesslogic.datatype;
using Umbraco.Core.Models;

namespace AST.ContentConveyor.DataTypeConverters
{
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

            if (!string.IsNullOrWhiteSpace(propertyTag.Value))
            {
                var dtd =
                    Services.DataTypeService.GetAllDataTypeDefinitions()
                        .FirstOrDefault(x => x.Name == propertyTag.Attribute("dataTypeName").Value);

                if (dtd != null)
                {
                    var values = PreValues.GetPreValues(dtd.Id);

                    foreach (DictionaryEntry de in values)
                    {
                        var value = (PreValue)de.Value;
                        if (string.Equals(value.Value, propertyTag.Value, StringComparison.CurrentCultureIgnoreCase))
                        {
                            result = value.Id.ToString();
                            break;
                        }
                    }
                }
                else
                {
                    result = propertyTag.Value;
                }
            }

            return result;
        }
    }
}