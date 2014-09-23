namespace AST.ContentPackage.EmbeddedContent
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using ContentPackage;
    using Umbraco.Core.Models;

    public class EmbeddedContentDataTypeConverter : BaseContentManagement, IDataTypeConverter
    {
        public void Export(Property property, XElement propertyTag, Dictionary<int, ObjectTypes> dependantNodes)
        {
            if (property.Value != null && !string.IsNullOrWhiteSpace(property.Value.ToString()))
            {
                var xml = XDocument.Parse(property.Value.ToString());

                var files = xml.Descendants("file").ToList();

                foreach (var f in files)
                {
                    int id;

                    if (int.TryParse(f.Value, out id))
                    {
                        f.Value = Services.MediaService.GetById(id).Key.ToString();

                        if (!dependantNodes.ContainsKey(id))
                        {
                            dependantNodes.Add(id, ObjectTypes.Media);
                        }
                    }
                }

                propertyTag.Value = xml.ToString();
            }
        }

        public string Import(XElement propertyTag)
        {
            var result = string.Empty;

            if (!string.IsNullOrWhiteSpace(propertyTag.Value))
            {
                var xml = XDocument.Parse(propertyTag.Value);

                var files = xml.Descendants("file").ToList();

                foreach (var f in files)
                {
                    var guid = new Guid(f.Value);
                    var id = Services.MediaService.GetById(guid).Id;
                    f.Value = id.ToString();
                }

                result = xml.ToString();
            }

            return result;
        }
    }
}