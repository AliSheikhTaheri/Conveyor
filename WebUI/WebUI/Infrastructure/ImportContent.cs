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
            var cs = services.ContentService;
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

                if (dataTypes.ContainsKey(dataTypeGuid) && !string.IsNullOrWhiteSpace(propertyTag.Attribute("guid").Value))
                {
                    DataTypeConverterImport(services, propertyTag, content, zip, dataTypes[dataTypeGuid]);
                }
                else
                {
                    content.SetValue(propertyTag.Name.ToString(), propertyTag.Value);
                }
            }

            cs.Save(content);
        }

        private void DataTypeConverterImport(ServiceContext services, XElement propertyTag, IContent content, ZipFile zip, string type)
        {
            var t = (IDataTypeConverter)Activator.CreateInstance(Type.GetType(type));

            t.Import(services, propertyTag, content, zip);
        }
    }
}