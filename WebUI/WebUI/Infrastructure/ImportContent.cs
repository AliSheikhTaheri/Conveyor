using System.Runtime.InteropServices;
using System.Web.Services.Description;
using umbraco;
using umbraco.presentation.webservices;

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
                    var nodeType = node.Attribute("objectType").Value;

                    var xRoot = new XmlRootAttribute { ElementName = node.Name.ToString(), IsNullable = true };
                    var xmlSerialiser = new XmlSerializer(typeof(Content), xRoot);

                    if (nodeType == ObjectTypes.Document.ToString())
                    {
                        CreateOrUpdateContent(zipFile, key, xmlSerialiser, node);
                    }
                    else if (nodeType == ObjectTypes.Media.ToString())
                    {
                        CreateOrUpdateMedia(zipFile, key, node);
                    }
                }

                var nodesWithSpecialProperties = root.Descendants().Where(x => x.Attribute("dataTypeGuid") != null
                    && !string.IsNullOrWhiteSpace(x.Attribute("dataTypeGuid").Value)
                    && SpecialDataTypes.ContainsKey(new Guid(x.Attribute("dataTypeGuid").Value)))
                    .GroupBy(x => x.Parent.Attribute("guid").Value)
                    .Select(
                            y => new
                            {
                                Guid = y.Key,
                                Properties = y.Select(x => x).ToList()
                            });

                foreach (var node in nodesWithSpecialProperties)
                {
                    var iContent = Services.ContentService.GetById(new Guid(node.Guid));

                    foreach (var prop in node.Properties)
                    {
                        var dataTypeGuid = new Guid(prop.Attribute("dataTypeGuid").Value);
                        var value = DataTypeConverterImport(prop, dataTypeGuid);
                        iContent.SetValue(prop.Name.ToString(), value);
                    }
                    
                    Services.ContentService.SaveAndPublish(iContent);
                }
            }
        }

        private void CreateOrUpdateMedia(ZipFile zipFile, Guid key, XElement node)
        {
            var media = Services.MediaService.GetById(key);

            if (media != null)
            {
                UpdateMedia(node, media, zipFile);
            }
            else
            {
                var name = node.Attribute("name").Value;
                var parentId = GetMediaParentId(node);

                var newMedia = Services.MediaService.CreateMedia(name, parentId, node.Name.ToString());

                UpdateMedia(node, newMedia, zipFile);
            }
        }

        private void CreateOrUpdateContent(ZipFile zipFile, Guid key, XmlSerializer xmlSerialiser, XElement node)
        {
            var content = Services.ContentService.GetById(key);
            var newContent = (Content)xmlSerialiser.Deserialize(node.CreateReader());
            if (content != null)
            {
                UpdateContent(node, content, newContent, zipFile);
            }
            else
            {
                var parentId = newContent.ParentId == -1
                    ? -1
                    : Services.ContentService.GetById(new Guid(newContent.ParentGuid)).Id;

                var newNode = Services.ContentService.CreateContent(newContent.Name, parentId, newContent.ContentTypeAlias,
                    User.GetCurrent().Id);
                newNode.Key = newContent.Key;
                UpdateContent(node, newNode, newContent, zipFile);
            }
        }

        private void UpdateMedia(XElement node, IMedia media, ZipFile zip)
        {
            media.Name = node.Attribute("name").Value;
            media.ParentId = GetMediaParentId(node);
            media.Key = new Guid(node.Attribute("guid").Value);

            foreach (var propertyTag in node.Elements())
            {
                var dataTypeGuid = new Guid(propertyTag.Attribute("dataTypeGuid").Value);

                if (dataTypeGuid == new Guid("5032a6e6-69e3-491d-bb28-cd31cd11086c"))
                {
                    var fileName = propertyTag.Attribute("fileName").Value;
                    var umbracoFile = propertyTag.Attribute("umbracoFile").Value;

                    media.SetValue(propertyTag.Name.ToString(), fileName, GetFileStream(umbracoFile, zip));
                }
                else if (!SpecialDataTypes.ContainsKey(dataTypeGuid))
                {
                    media.SetValue(propertyTag.Name.ToString(), propertyTag.Value);
                }
            }

            Services.MediaService.Save(media);
        }

        private MemoryStream GetFileStream(string umbracoFile, ZipFile zip)
        {
            umbracoFile = umbracoFile.Remove(0, 1);

            var memoryStream = new MemoryStream();

            var zipEntry = zip.Entries.SingleOrDefault(x => x.FileName == umbracoFile);
            zipEntry.Extract(memoryStream);

            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }

        private int GetMediaParentId(XElement node)
        {
            var parentId = node.Attribute("parentGuid").Value == "-1" ? -1 : 0;

            if (parentId == 0)
            {
                var m = Services.MediaService.GetById(new Guid(node.Attribute("parentGuid").Value));

                parentId = m != null ? m.Id : -1;
            }
            return parentId;
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
                if (dataTypeGuid == new Guid("5032a6e6-69e3-491d-bb28-cd31cd11086c"))
                {
                    var fileName = propertyTag.Attribute("fileName").Value;
                    var umbracoFile = propertyTag.Attribute("umbracoFile").Value;

                    content.SetValue(propertyTag.Name.ToString(), fileName, GetFileStream(umbracoFile, zip));
                }
                else if (!dataTypes.ContainsKey(dataTypeGuid))
                {
                    content.SetValue(propertyTag.Name.ToString(), value);
                }
            }

            cs.SaveAndPublish(content);
        }

        private string DataTypeConverterImport(XElement propertyTag, Guid guid)
        {
            var type = SpecialDataTypes[guid];

            var dataTypeConverter = GetDataTypeConverterInterface(type);

            return dataTypeConverter.Import(propertyTag);
        }
    }
}