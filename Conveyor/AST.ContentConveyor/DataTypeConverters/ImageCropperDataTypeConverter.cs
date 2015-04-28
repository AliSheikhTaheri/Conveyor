using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Umbraco.Core.Models;

namespace AST.ContentConveyor.DataTypeConverters
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
