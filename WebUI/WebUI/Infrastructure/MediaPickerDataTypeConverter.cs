using Ionic.Zip;

namespace WebUI.Infrastructure
{
    using System;
    using System.IO;
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

                var media = services.MediaService.GetById((int)property.Value);
                var umbracoFile = media.GetValue<string>("umbracoFile");
                propertyTag.Add(new XAttribute("name", media.Name));
                propertyTag.Add(new XAttribute("nodeTypeAlias", media.ContentType.Alias));
                propertyTag.Add(new XAttribute("guid", media.Key));
                propertyTag.Add(new XAttribute("parentGuid", media.Parent() == null ? "-1" : media.Parent().Key.ToString()));
                propertyTag.Add(new XAttribute("umbracoFile", umbracoFile));
                propertyTag.Add(new XAttribute("fileName", umbracoFile.Split('/').Last()));
            }
        }

        public void Import(ServiceContext services, XElement propertyTag, IContent content, ZipFile zip)
        {
            var propGuid = new Guid(propertyTag.Attribute("guid").Value);
            var media = services.MediaService.GetById(propGuid);

            if (media != null)
            {
                content.SetValue(propertyTag.Name.ToString(), media.Id.ToString());
            }
            else
            {
                // create media and assign it.
                var name = propertyTag.Attribute("name").Value;
                var nodeTypeAlias = propertyTag.Attribute("nodeTypeAlias").Value;
                var fileName = propertyTag.Attribute("fileName").Value;
                var parent = propertyTag.Attribute("parentGuid").Value == "-1"
                    ? -1
                    : services.MediaService.GetById(new Guid(propertyTag.Attribute("parentGuid").Value)).Id;


                var mediaNode = services.MediaService.CreateMedia(name, parent, nodeTypeAlias);

                var memoryStream = new MemoryStream();

                var zipEntry = zip.Entries.SingleOrDefault(x => x.FileName == fileName);
                zipEntry.Extract(memoryStream);

                memoryStream.Seek(0, SeekOrigin.Begin);

                mediaNode.SetValue("umbracoFile", fileName, memoryStream);

                services.MediaService.Save(mediaNode);

                content.SetValue(propertyTag.Name.ToString(), mediaNode.Id);
            }
        }
    }
}