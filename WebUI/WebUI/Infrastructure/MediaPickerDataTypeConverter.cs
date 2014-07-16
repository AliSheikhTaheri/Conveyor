namespace WebUI.Infrastructure
{
    using System.Linq;
    using System.Xml.Linq;
    using Umbraco.Core.Models;
    using Umbraco.Core.Services;

    public class MediaPickerDataTypeConverter : IDataTypeConverter
    {
        public void Export(Property property, ServiceContext services, XElement propertyTag)
        {
            if (property.Value != null && !string.IsNullOrWhiteSpace(property.Value.ToString()))
            {
                propertyTag.Value = string.Empty;

                var media = services.MediaService.GetById((int) property.Value);
                var umbracoFile = media.GetValue<string>("umbracoFile");
                propertyTag.Add(new XAttribute("name", media.Name));
                propertyTag.Add(new XAttribute("nodeTypeAlias", media.ContentType.Alias));
                propertyTag.Add(new XAttribute("guid", media.Key));
                propertyTag.Add(new XAttribute("parentGuid", media.Parent() == null ? "-1" : media.Parent().Key.ToString()));
                propertyTag.Add(new XAttribute("umbracoFile", umbracoFile));
                propertyTag.Add(new XAttribute("fileName", umbracoFile.Split('/').Last()));
            }
        }

        public void Import()
        {
        }
    }
}