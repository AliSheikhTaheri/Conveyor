namespace WebUI.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Linq;
    using Umbraco.Core;
    using Umbraco.Core.Models;
    using Umbraco.Core.Services;

    public class ExportContent
    {
        #region Methods

        public XDocument SerialiseToXml(string ids)
        {
            if (ids.Length > 0)
            {
                var services = ApplicationContext.Current.Services;

                var cs = services.ContentService;

                var dataTypes = new Config().GetDataTypes();

                var nodes = ids.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                var xml = new XElement("Root");

                foreach (var node in nodes)
                {
                    var id = int.Parse(node);

                    var content = cs.GetById(id);
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
                        new XAttribute("guid", content.Key));

                    var propertyTypes = content.PropertyTypes.ToArray();
                    var count = 0;

                    foreach (var property in content.Properties)
                    {
                        var tag = property.ToXml();
                        tag.Add(new XAttribute("dataTypeGuid", propertyTypes.ElementAt(count).DataTypeId));

                        var guid = propertyTypes.ElementAt(count).DataTypeId;

                        if (dataTypes.ContainsKey(guid))
                        {
                            DataTableConverter(property, services, tag, dataTypes[guid]);
                        }

                        currentContent.Add(tag);
                        count++;
                    }

                    xml.Add(currentContent);
                }

                var xdocument = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
                xdocument.Add(xml);

                return xdocument;
            }

            return null;
        }

        private void DataTableConverter(Property property, ServiceContext services, XElement propertyTag, string type)
        {
            var t = (IDataTypeConverter)Activator.CreateInstance(Type.GetType(type));

            t.Export(property, services, propertyTag);
        }

        public IEnumerable<string> GetListOfAssets(XDocument xdoc)
        {
            return xdoc.Descendants()
                .Where(x => x.Attribute("umbracoFile") != null && !string.IsNullOrWhiteSpace(x.Attribute("umbracoFile").Value))
                .Select(x => x.Attribute("umbracoFile").Value);
        }

        #endregion
    }
}