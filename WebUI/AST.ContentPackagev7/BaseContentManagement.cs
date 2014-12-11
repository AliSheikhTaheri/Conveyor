namespace AST.ContentPackagev7
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Linq;

    using AST.ContentPackagev7.DataTypeConverters;
    using AST.ContentPackagev7.Enums;

    using Umbraco.Core;
    using Umbraco.Core.Models;
    using Umbraco.Core.Services;

    public abstract class BaseContentManagement
    {
        #region Constructor

        protected BaseContentManagement()
        {
            SpecialDataTypes = new Config().GetSpecialDataTypes();
            Services = ApplicationContext.Current.Services;
        }

        #endregion

        #region Fields

        public Dictionary<Guid, string> SpecialDataTypes { get; set; }

        public ServiceContext Services { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Load external classes and convert them into IDataTypeConverter 
        /// </summary>
        /// <param name="type">class name, assembly name</param>
        /// <returns>return IDataTypeConverter</returns>
        protected IDataTypeConverter GetDataTypeConverterInterface(string type)
        {
            if (string.IsNullOrWhiteSpace(type) || Type.GetType(type) == null)
            {
                throw new Exception(
                    string.Format(
                        "The system cannot find {0}. Make sure the assembly name is right or has been included in the config file.!",
                        type));
            }

            return (IDataTypeConverter)Activator.CreateInstance(Type.GetType(type));
        }

        /// <summary>
        /// Serialize IContent to XElement and adds dependent nodes 
        /// </summary>
        /// <param name="content">Umbraco IContent object</param>
        /// <param name="dependantNodes">this function will add dependent nodes to this collection</param>
        /// <returns>returns serialized version of IContent as XElement</returns>
        public XElement SerialiseContent(IContent content, Dictionary<int, ObjectTypes> dependantNodes = null)
        {
            dependantNodes = dependantNodes ?? new Dictionary<int, ObjectTypes>();

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
                new XAttribute("objectType", ObjectTypes.Document),
                new XAttribute("published", content.Published));

            var propertyTypes = content.PropertyTypes.ToArray();
            var count = 0;

            foreach (var property in content.Properties)
            {
                var tag = property.ToXml();
                var propertyType = propertyTypes.ElementAt(count);
                tag.Add(new XAttribute("dataTypeGuid", propertyType.DataTypeId));
                tag.Add(new XAttribute("dataTypeName", Services.DataTypeService.GetDataTypeDefinitionById(propertyType.DataTypeDefinitionId).Name));

                var guid = propertyTypes.ElementAt(count).DataTypeId;

                if (guid == new Guid(Constants.UploadDataTypeGuid))
                {
                    var umbracoFile = property.Value.ToString();
                    tag.Add(
                        new XAttribute("umbracoFile", umbracoFile),
                        new XAttribute("fileName", umbracoFile.Split('/').Last()),
                        new XAttribute("objectType", ObjectTypes.File));
                }
                else if (SpecialDataTypes.ContainsKey(guid))
                {
                    DataTypeConverterExport(property, tag, dependantNodes, SpecialDataTypes[guid]);
                }

                currentContent.Add(tag);
                count++;
            }

            return currentContent;
        }

        /// <summary>
        /// Serialize IMedia to XElement and adds dependent nodes
        /// </summary>
        /// <param name="media">Umbraco IMedia object</param>
        /// <param name="dependantNodes">this function will add dependent nodes to this collection</param>
        /// <returns>returns serialized version of IMedia as XElement</returns>
        public XElement SerialiseMedia(IMedia media, Dictionary<int, ObjectTypes> dependantNodes = null)
        {
            var nodeName = media.ContentType.Alias.ToSafeAliasWithForcingCheck();

            var node = new XElement(nodeName,
                new XAttribute("name", media.Name),
                new XAttribute("nodeTypeAlias", media.ContentType.Alias),
                new XAttribute("guid", media.Key),
                new XAttribute("sortOrder", media.SortOrder),
                new XAttribute("parentGuid", media.Parent() == null ? "-1" : media.Parent().Key.ToString()),
                new XAttribute("level", media.Level),
                new XAttribute("objectType", ObjectTypes.Media));


            var propertyTypes = media.PropertyTypes.Where(x => !Constants.MediaDefaultProperties.Contains(x.Alias)).ToArray();
            var count = 0;

            foreach (var property in media.Properties.Where(x => !Constants.MediaDefaultProperties.Contains(x.Alias)))
            {
                var tag = property.ToXml();
                var propertyType = propertyTypes.ElementAt(count);
                tag.Add(new XAttribute("dataTypeGuid", propertyType.DataTypeId));
                tag.Add(new XAttribute("dataTypeName", Services.DataTypeService.GetDataTypeDefinitionById(propertyType.DataTypeDefinitionId).Name));

                var guid = propertyTypes.ElementAt(count).DataTypeId;
                if (guid == new Guid(Constants.UploadDataTypeGuid))
                {
                    var umbracoFile = property.Value.ToString();
                    tag.Add(
                        new XAttribute("umbracoFile", umbracoFile),
                        new XAttribute("fileName", umbracoFile.Split('/').Last()),
                        new XAttribute("objectType", ObjectTypes.File));
                }
                else if (SpecialDataTypes.ContainsKey(guid))
                {
                    DataTypeConverterExport(property, tag, dependantNodes, SpecialDataTypes[guid]);
                }

                node.Add(tag);
                count++;
            }

            return node;
        }

        #endregion

        #region Helpers

        private void DataTypeConverterExport(Property property, XElement propertyTag, Dictionary<int, ObjectTypes> dependantNodes, string type)
        {
            var t = GetDataTypeConverterInterface(type);

            t.Export(property, propertyTag, dependantNodes);
        }

        #endregion
    }
}