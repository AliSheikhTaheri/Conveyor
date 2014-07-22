using System.Runtime.InteropServices;
using umbraco;

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

    internal class ImportContent : BaseContentManagement
    {
        internal void Import(HttpPostedFileBase zipFile)
        {
            if (zipFile.ContentLength > 0)
            {
                using (var zip = ZipFile.Read(zipFile.InputStream))
                {
                    XDocument xdoc;

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

        internal void ImportNodes(XDocument xml, ZipFile zipFile)
        {
            var root = xml.Root;

            if (root != null)
            {
                foreach (var node in root.Elements())
                {
                    var key = new Guid(node.Attribute("guid").Value);
                    var nodeType = uQuery.GetUmbracoObjectType(key);

                    var xRoot = new XmlRootAttribute { ElementName = node.Name.ToString(), IsNullable = true };
                    var xmlSerialiser = new XmlSerializer(typeof(Content), xRoot);

                    if (nodeType == uQuery.UmbracoObjectType.Document)
                    {
                        var content = Services.ContentService.GetById(key);
                        var newContent = (Content)xmlSerialiser.Deserialize(node.CreateReader());
                        if (content != null)
                        {
                            UpdateContent(node, content, newContent, zipFile);
                        }
                        else
                        {
                            var parentId = newContent.ParentId == -1 ? -1 : Services.ContentService.GetById(new Guid(newContent.ParentGuid)).Id;

                            var newNode = Services.ContentService.CreateContent(newContent.Name, parentId, newContent.ContentTypeAlias, User.GetCurrent().Id);
                            newNode.Key = newContent.Key;
                            UpdateContent(node, newNode, newContent, zipFile);
                        }
                    }
                    else if (nodeType == uQuery.UmbracoObjectType.Media)
                    {
                        var media = Services.MediaService.GetById(key);

                        if (media != null)
                        {
                            UpdateMedia(node, media, zipFile);
                        }
                        else
                        {
                            var name = node.Attribute("name").Value;
                            var parentId = node.Attribute("parentGuid").Value == "-1"
                                ? -1
                                : Services.MediaService.GetById(new Guid(node.Attribute("parentGuid").Value)).Id;

                            var newMedia = Services.MediaService.CreateMedia(name, parentId, node.Name.ToString());

                            UpdateMedia(node, newMedia, zipFile);
                        }
                    }

                    //todo: go through all the special datatypes 
                }
            }
        }

        private void UpdateMedia(XElement node, IMedia media, ZipFile zip)
        {
            var fileName = node.Attribute("umbracoFile").Value;
            fileName = fileName.Remove(0, 1);
            var parentId = node.Attribute("parentGuid").Value == "-1"
                                ? -1
                                : Services.MediaService.GetById(new Guid(node.Attribute("parentGuid").Value)).Id;

            media.Name = node.Attribute("name").Value;
            media.ParentId = parentId;
            media.Key = new Guid(node.Attribute("guid").Value);

            var memoryStream = new MemoryStream();

            var zipEntry = zip.Entries.SingleOrDefault(x => x.FileName == fileName);
            zipEntry.Extract(memoryStream);

            memoryStream.Seek(0, SeekOrigin.Begin);

            media.SetValue("umbracoFile", fileName, memoryStream);

            Services.MediaService.Save(media);
        }

        private void UpdateContent(XElement node, IContent content, Content newContent, ZipFile zip)
        {
            var cs = Services.ContentService;
            var dataTypes = new Config().GetDataTypes();

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

                var value = propertyTag.Value;

                if (dataTypes.ContainsKey(dataTypeGuid))
                {
                    //value = DataTypeConverterImport(propertyTag, dataTypes[dataTypeGuid]);
                    continue;
                }

                content.SetValue(propertyTag.Name.ToString(), value);
            }

            cs.Save(content);
        }

        private string DataTypeConverterImport(XElement propertyTag, string type)
        {
            var t = GetDataTypeConverterInterface(type);

            return t.Import(propertyTag);
        }
    }
}