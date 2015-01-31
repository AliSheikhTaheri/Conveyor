namespace AST.ContentConveyor7.DataTypeConverters
{
    using System.Collections.Generic;
    using System.Xml.Linq;

    using AST.ContentConveyor7.Enums;

    using Umbraco.Core.Models;

    public interface IDataTypeConverter
    {
        void Export(Property property, XElement propertyTag, Dictionary<int, ObjectTypes> dependantNodes);

        string Import(XElement propertyTag);
    }
}
