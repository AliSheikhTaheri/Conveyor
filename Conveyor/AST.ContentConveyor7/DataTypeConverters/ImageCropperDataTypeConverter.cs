using System;
using System.Collections.Generic;
using System.Xml.Linq;
using AST.ContentConveyor7.Enums;
using Umbraco.Core.Models;

namespace AST.ContentConveyor7.DataTypeConverters
{
    public class ImageCropperDataTypeConverter : BaseContentManagement, IDataTypeConverter
    {
        public void Export(Property property, XElement propertyTag, Dictionary<int, ObjectTypes> dependantNodes)
        {
            throw new NotImplementedException();
        }

        public string Import(XElement propertyTag)
        {
            throw new NotImplementedException();
        }
    }
}
