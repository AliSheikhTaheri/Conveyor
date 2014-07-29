namespace WebUI.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Linq;
    using umbraco;
    using Umbraco.Core;
    using Umbraco.Core.Models;
    using Umbraco.Core.Services;

    public abstract class BaseContentManagement
    {
        protected BaseContentManagement()
        {
            SpecialDataTypes = new Config().GetDataTypes();
            Services = ApplicationContext.Current.Services;
        }

        public Dictionary<Guid, string> SpecialDataTypes { get; set; }

        public ServiceContext Services { get; set; }

        protected IDataTypeConverter GetDataTypeConverterInterface(string type)
        {
            return (IDataTypeConverter)Activator.CreateInstance(Type.GetType(type));
        }

        public XElement SerialiseContent(IContent content, Dictionary<int, ObjectTypes> dependantNdoes = null)
        {
            dependantNdoes = dependantNdoes ?? new Dictionary<int, ObjectTypes>();

            var nodeName = content.ContentType.Alias.ToSafeAliasWithForcingCheck();

            var currentContent = new XElement(nodeName,
                new XAttribute("nodeName", content.Name),
                new XAttribute("nodeType", content.ContentType.Id),
                new XAttribute("creatorName", content.GetCreatorProfile().Name),
                new XAttribute("writerName", content.GetWriterProfile().Name),
                new XAttribute("writerID", content.WriterId),
                new XAttribute("templateID", content.Template == null ? "0" : content.Template.Id.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("nodeTypeAlias", content.ContentType.Alias),
                new XAttribute("id", content.Id),
                new XAttribute("parentID", content.Level > 1 ? content.ParentId : -1),
                new XAttribute("level", content.Level),
                new XAttribute("creatorID", content.CreatorId),
                new XAttribute("sortOrder", content.SortOrder),
                new XAttribute("createDate", content.CreateDate.ToString("s")),
                new XAttribute("updateDate", content.UpdateDate.ToString("s")),
                new XAttribute("path", content.Path),
                new XAttribute("isDoc", string.Empty),
                new XAttribute("releaseDate", content.ReleaseDate != null ? content.ReleaseDate.Value.ToString("s") : DateTime.MinValue.ToString("s")),
                new XAttribute("expireDate", content.ExpireDate != null ? content.ExpireDate.Value.ToString("s") : DateTime.MinValue.ToString("s")),
                new XAttribute("parentGuid", content.Level > 1 ? content.Parent().Key.ToString() : string.Empty),
                new XAttribute("guid", content.Key),
                new XAttribute("objectType", ObjectTypes.Document));

            var propertyTypes = content.PropertyTypes.ToArray();
            var count = 0;

            foreach (var property in content.Properties)
            {
                var tag = property.ToXml();
                tag.Add(new XAttribute("dataTypeGuid", propertyTypes.ElementAt(count).DataTypeId));

                var guid = propertyTypes.ElementAt(count).DataTypeId;

                if (guid == new Guid("5032a6e6-69e3-491d-bb28-cd31cd11086c"))
                {
                    var umbracoFile = property.Value.ToString();
                    tag.Add(
                        new XAttribute("umbracoFile", umbracoFile),
                        new XAttribute("fileName", umbracoFile.Split('/').Last()),
                        new XAttribute("objectType", ObjectTypes.File));
                }
                else if (SpecialDataTypes.ContainsKey(guid))
                {
                    DataTypeConverterExport(property, tag, dependantNdoes, SpecialDataTypes[guid]);
                }

                currentContent.Add(tag);
                count++;
            }

            return currentContent;
        }

        public XElement SerialiseMedia(IMedia media, Dictionary<int, ObjectTypes> dependantNdoes = null)
        {
            var nodeName = media.ContentType.Alias.ToSafeAliasWithForcingCheck();

            var node = new XElement(nodeName,
                new XAttribute("name", media.Name),
                new XAttribute("nodeTypeAlias", media.ContentType.Alias),
                new XAttribute("guid", media.Key),
                new XAttribute("parentGuid", media.Parent() == null ? "-1" : media.Parent().Key.ToString()),
                new XAttribute("level", media.Level),
                new XAttribute("objectType", ObjectTypes.Media));


            var propertyTypes = media.PropertyTypes.Where(x => !Constants.MediaDefaultProperties.Contains(x.Alias)).ToArray();
            var count = 0;

            foreach (var property in media.Properties.Where(x => !Constants.MediaDefaultProperties.Contains(x.Alias)))
            {
                var tag = property.ToXml();
                tag.Add(new XAttribute("dataTypeGuid", propertyTypes.ElementAt(count).DataTypeId));

                var guid = propertyTypes.ElementAt(count).DataTypeId;
                if (guid == new Guid("5032a6e6-69e3-491d-bb28-cd31cd11086c"))
                {
                    var umbracoFile = property.Value.ToString();
                    tag.Add(
                        new XAttribute("umbracoFile", umbracoFile),
                        new XAttribute("fileName", umbracoFile.Split('/').Last()),
                        new XAttribute("objectType", ObjectTypes.File));
                }
                else if (SpecialDataTypes.ContainsKey(guid))
                {
                    DataTypeConverterExport(property, tag, dependantNdoes, SpecialDataTypes[guid]);
                }

                node.Add(tag);
                count++;
            }

            return node;
        }

        private void DataTypeConverterExport(Property property, XElement propertyTag, Dictionary<int, ObjectTypes> dependantNdoes, string type)
        {
            var t = GetDataTypeConverterInterface(type);

            t.Export(property, propertyTag, dependantNdoes);
        }
    }
}