namespace WebUI.Infrastructure
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Xml.Linq;
    using System.Xml.Serialization;
    using Ionic.Zip;
    using umbraco.BusinessLogic;
    using Umbraco.Core;
    using Umbraco.Core.Models;
    using Umbraco.Core.Services;

    public class ImportContent
    {
        public void Import(HttpPostedFileBase zipFile)
        {
            if (zipFile.ContentLength > 0)
            {
                using (var zip = ZipFile.Read(zipFile.InputStream))
                {
                    var xdoc = new XDocument();

                    var contentXmlZipEntry = zip.Entries.FirstOrDefault(x => x.FileName == "content.xml");

                    using (var ms = new MemoryStream())
                    {
                        contentXmlZipEntry.Extract(ms);
                        ms.Seek(0, SeekOrigin.Begin);
                        xdoc = XDocument.Load(ms);
                    }

                    ImportNodes(xdoc, zip);
                }
            }
        }

        public void ImportNodes(XDocument xml, ZipFile zipFile)
        {
            var root = xml.Root;

            if (root != null)
            {
                var services = ApplicationContext.Current.Services;
                var cs = services.ContentService;

                foreach (var node in root.Elements())
                {
                    var key = new Guid(node.Attribute("guid").Value);

                    var content = cs.GetById(key);

                    var xRoot = new XmlRootAttribute { ElementName = node.Name.ToString(), IsNullable = true };
                    var xmlSerialiser = new XmlSerializer(typeof(Content), xRoot);
                    var newContent = (Content)xmlSerialiser.Deserialize(node.CreateReader());

                    if (content != null)
                    {
                        UpdateContent(node, content, newContent, services, zipFile);
                    }
                    else
                    {
                        var parentId = newContent.ParentId == -1 ? -1 : cs.GetById(new Guid(newContent.ParentGuid)).Id;

                        var newNode = cs.CreateContent(newContent.Name, parentId, newContent.ContentTypeAlias, User.GetCurrent().Id);
                        newNode.Key = newContent.Key;
                        UpdateContent(node, newNode, newContent, services, zipFile);
                    }
                }
            }
        }

        private void UpdateContent(XElement node, IContent content, Content newContent, ServiceContext services, ZipFile zip)
        {
            var ms = services.MediaService;
            var cs = services.ContentService;

            content.Name = newContent.Name;
            content.CreatorId = User.GetCurrent().Id;
            content.SortOrder = newContent.SortOrder;
            content.WriterId = User.GetCurrent().Id;
            content.Template.Id = newContent.TemplateId;

            if (newContent.ReleaseDate != DateTime.MinValue)
            {
                content.ReleaseDate = newContent.ReleaseDate;
            }

            if (newContent.ExpireDate != DateTime.MinValue)
            {
                content.ExpireDate = newContent.ExpireDate;
            }

            foreach (var propertyTag in node.Elements())
            {
                var dataTypeGuid = new Guid(propertyTag.Attribute("dataTypeGuid").Value);

                if (IsSpecialProperty(dataTypeGuid) && !string.IsNullOrWhiteSpace(propertyTag.Attribute("guid").Value))
                {
                    var propGuid = new Guid(propertyTag.Attribute("guid").Value);
                    var media = ms.GetById(propGuid);

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
                            : ms.GetById(new Guid(propertyTag.Attribute("parentGuid").Value)).Id;


                        var mediaNode = ms.CreateMedia(name, parent, nodeTypeAlias);

                        var memoryStream = new MemoryStream();

                        var zipEntry = zip.Entries.SingleOrDefault(x => x.FileName == fileName);
                        zipEntry.Extract(memoryStream);

                        memoryStream.Seek(0, SeekOrigin.Begin);

                        mediaNode.SetValue("umbracoFile", fileName, memoryStream);

                        ms.Save(mediaNode);

                        content.SetValue(propertyTag.Name.ToString(), mediaNode.Id);
                    }
                }
                else
                {
                    content.SetValue(propertyTag.Name.ToString(), propertyTag.Value);
                }
            }

            cs.Save(content);
        }

        public bool IsSpecialProperty(Guid guid)
        {
            return guid.Equals(new Guid("ead69342-f06d-4253-83ac-28000225583b"));
        }
    }
}