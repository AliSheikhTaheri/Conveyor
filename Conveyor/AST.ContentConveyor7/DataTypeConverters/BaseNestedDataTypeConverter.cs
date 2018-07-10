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
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Base class for recursive converters that process a chunk of data (usually JSON)
    /// and need to internally invoke other Type Converters
    /// </summary>
    /// <remarks>
    /// Provides two wrapper methods for invoking the Export and Import operation on a nested data type
    /// </remarks>
    public class BaseNestedTypeConverter : BaseContentManagement
    {
        /// <summary>
        /// Invokes the Type Converter Export for an individual value inside the nested data type
        /// </summary>
        protected string NestedExport(IDataTypeConverter typeConverter, string value, PropertyType propertyType, Dictionary<int, ObjectTypes> dependantNodes)
        {
            // recursively invoke the TypeConverter
            Property property = new Property(propertyType, value);
            XElement propertyTag = property.ToXml();
            typeConverter.Export(property, propertyTag, dependantNodes);
            return propertyTag.Value;
        }

        /// <summary>
        /// Invokes the Type Converter Import for an individual value inside the nested data type
        /// </summary>
        protected string NestedImport(IDataTypeConverter typeConverter, string value)
        {
            // recursively invoke the TypeConverter
            XElement propertyTag = new XElement("Property", new XText(value));
            return typeConverter.Import(propertyTag);
        }
    }
}