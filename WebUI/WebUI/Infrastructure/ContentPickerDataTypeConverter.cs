namespace WebUI.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;
    using Ionic.Zip;
    using Umbraco.Core.Models;
    using Umbraco.Core.Services;

    public class ContentPickerDataTypeConverter : BaseContentManagement, IDataTypeConverter
    {
        public void Export(Property property, XElement propertyTag, Dictionary<Guid, UmbracoObjectTypes> dependantNodes)
        {
            if (property.Value != null && !string.IsNullOrWhiteSpace(property.Value.ToString()))
            {
                var content = Services.ContentService.GetById((int)property.Value);
                propertyTag.Value = content.Key.ToString();

                if (!dependantNodes.ContainsKey(content.Key))
                {
                    dependantNodes.Add(content.Key, UmbracoObjectTypes.Document);
                }
            }
        }

        public string Import(XElement propertyTag)
        {
            var guid = new Guid(propertyTag.Value);
            var id = Services.ContentService.GetById(guid).Id;

            return id.ToString();
        }
    }
}