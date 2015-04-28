using System.Configuration;
using System.Linq;
using Umbraco.Core.Configuration;

namespace AST.ContentConveyor7.DataTypeConverters
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;

    using AST.ContentConveyor7;
    using AST.ContentConveyor7.Enums;
    using AST.ContentConveyor7.Utilities;

    using Umbraco.Core.Models;

    public class MediaPickerDataTypeConverter : BaseContentManagement, IDataTypeConverter
    {
        public void Export(Property property, XElement propertyTag, Dictionary<int, ObjectTypes> dependantNodes)
        {
            if (property.Value != null && !string.IsNullOrWhiteSpace(property.Value.ToString()))
            {
                var id = int.Parse(property.Value.ToString());
                var media = Services.MediaService.GetById(id);

                var uploadFieldAlias = GetUploadFieldAlias(media);

                // TODO for v6
                if (media != null && FileHelpers.FileExists(media.GetValue(uploadFieldAlias).ToString())) 
                {
                    propertyTag.Value = media.Key.ToString();

                    if (!dependantNodes.ContainsKey(media.Id))
                    {
                        dependantNodes.Add(media.Id, ObjectTypes.Media);
                    }
                }
            }
        }

        public string Import(XElement propertyTag)
        {
            var result = string.Empty;

            if (!string.IsNullOrWhiteSpace(propertyTag.Value))
            {
                var guid = new Guid(propertyTag.Value);
                var id = Services.MediaService.GetById(guid).Id;
                result = id.ToString();
            }

            return result;
        }

        private string GetUploadFieldAlias(IContentBase node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            var uploadFields = UmbracoConfig.For.UmbracoSettings().Content.ImageAutoFillProperties.ToList();
            if (uploadFields == null || uploadFields.All(f => string.IsNullOrEmpty(f.Alias)))
            {
                throw new ConfigurationErrorsException("Expected /content/imaging/autoFillImageProperties/uploadField alias attribute");
            }

            if (!uploadFields.Any(f => !string.IsNullOrEmpty(f.Alias) && node.HasProperty(f.Alias)))
            {
                throw new Exception(string.Format("Could not determine uploadField alias for node with id: {0}", node.Id));
            }

            return uploadFields.First(f => !string.IsNullOrEmpty(f.Alias) && node.HasProperty(f.Alias)).Alias;
        }
    }
}