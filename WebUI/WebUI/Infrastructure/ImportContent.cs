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
    using Umbraco.Core.Models;

    internal class ImportContent : BaseContentManagement
    {
        internal void Import(HttpPostedFileBase zipFile)
        {
            if (zipFile.ContentLength > 0)
            {
                using (var zip = ZipFile.Read(zipFile.InputStream))
                {
                    XDocument xdoc;

                    var contentXmlZipEntry = zip.Entries.FirstOrDefault(x => x.FileName == Constants.ContentFileName);

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

        #region Content
        private void CreateOrUpdateContent(ZipFile zipFile, Guid key, XmlSerializer xmlSerialiser, XElement node)
        {
            var content = Services.ContentService.GetById(key);
            var newContent = (Content)xmlSerialiser.Deserialize(node.CreateReader());
            if (content != null)
            {
                SaveContent(node, content, newContent, zipFile);
            }
            else
            {
                var parentId = GetContentParentId(newContent);

                var newNode = Services.ContentService.CreateContent(newContent.Name, parentId, newContent.ContentTypeAlias,
                    User.GetCurrent().Id);
                newNode.Key = newContent.Key;
                SaveContent(node, newNode, newContent, zipFile);
            }
        }

        private void CreateOrUpdateMedia(ZipFile zipFile, Guid key, XElement node)
        {
            var media = Services.MediaService.GetById(key);

            if (media != null)
            {
                SaveMedia(node, media, zipFile);
            }
            else
            {
                var name = node.Attribute("name").Value;
                var parentId = GetMediaParentId(node);

                var newMedia = Services.MediaService.CreateMedia(name, parentId, node.Name.ToString());

                SaveMedia(node, newMedia, zipFile);
            }
        }

        private void SaveContent(XElement node, IContent content, Content newContent, ZipFile zip)
        {
            var cs = Services.ContentService;
            var dataTypes = new Config().GetSpecialDataTypes();

            content.ParentId = GetContentParentId(newContent);
            content.SortOrder = newContent.SortOrder;
            content.Name = newContent.Name;
            content.CreatorId = User.GetCurrent().Id;
            content.WriterId = User.GetCurrent().Id;

            if (content.Template != null)
            {
                content.Template.Id = newContent.TemplateId;
            }

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
                if (dataTypeGuid == new Guid(Constants.UploadDataTypeGuid))
                {
                    var fileName = propertyTag.Attribute("fileName").Value;
                    var umbracoFile = propertyTag.Attribute("umbracoFile").Value;

                    if (!string.IsNullOrWhiteSpace(umbracoFile))
                    {
                        content.SetValue(propertyTag.Name.ToString(), fileName, GetFileStream(umbracoFile, zip));
                    }
                    else
                    {
                        content.SetValue(propertyTag.Name.ToString(), string.Empty);
                    }
                }
                else if (!dataTypes.ContainsKey(dataTypeGuid))
                {
                    content.SetValue(propertyTag.Name.ToString(), value);
                }
            }

            cs.SaveAndPublish(content);
        }
        #endregion

        #region Media

        private int GetContentParentId(Content content)
        {
            var parentId = content.ParentId == -1 ? -1 : 0;

            if (parentId == 0)
            {
                var m = Services.ContentService.GetById(new Guid(content.ParentGuid));

                parentId = m != null ? m.Id : -1;
            }

            return parentId;
        }

        private void SaveMedia(XElement node, IMedia media, ZipFile zip)
        {
            media.Name = node.Attribute("name").Value;
            media.ParentId = GetMediaParentId(node);
            media.Key = new Guid(node.Attribute("guid").Value);

            int sortOrder;
            if (int.TryParse(node.Attribute("sortOrder").Value, out sortOrder))
            {
                media.SortOrder = sortOrder;
            }

            foreach (var propertyTag in node.Elements())
            {
                var dataTypeGuid = new Guid(propertyTag.Attribute("dataTypeGuid").Value);

                if (dataTypeGuid == new Guid(Constants.UploadDataTypeGuid))
                {
                    var fileName = propertyTag.Attribute("fileName").Value;
                    var umbracoFile = propertyTag.Attribute("umbracoFile").Value;

                    if (!string.IsNullOrWhiteSpace(umbracoFile))
                    {
                        media.SetValue(propertyTag.Name.ToString(), fileName, GetFileStream(umbracoFile, zip));
                    }
                    else
                    {
                        media.SetValue(propertyTag.Name.ToString(), string.Empty);
                    }
                }
                else if (!SpecialDataTypes.ContainsKey(dataTypeGuid))
                {
                    media.SetValue(propertyTag.Name.ToString(), propertyTag.Value);
                }
            }

            Services.MediaService.Save(media);
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

        #endregion

        #region Helpers

        private void ImportNodes(XDocument xml, ZipFile zipFile)
        {
            var root = xml.Root;

            if (root != null)
            {
                foreach (var node in root.Elements().OrderBy(x => int.Parse(x.Attribute("level").Value)))
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

        private MemoryStream GetFileStream(string umbracoFile, ZipFile zip)
        {
            umbracoFile = umbracoFile.Remove(0, 1);

            var memoryStream = new MemoryStream();

            var zipEntry = zip.Entries.SingleOrDefault(x => x.FileName == umbracoFile);
            zipEntry.Extract(memoryStream);

            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }

        private string DataTypeConverterImport(XElement propertyTag, Guid guid)
        {
            var type = SpecialDataTypes[guid];

            var dataTypeConverter = GetDataTypeConverterInterface(type);

            return dataTypeConverter.Import(propertyTag);
        }

        #endregion
    }
}