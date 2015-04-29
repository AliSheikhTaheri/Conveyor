using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using AST.ContentConveyor.Utilities;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Models;

namespace AST.ContentConveyor.DataTypeConverters
{
    public class ImageCropperDataTypeConverter : BaseContentManagement, IDataTypeConverter, IUploadDataTypeConverter
    {
        public void Export(Property property, XElement propertyTag, Dictionary<int, ObjectTypes> dependantNodes)
        {
            if (property.Value != null && !string.IsNullOrWhiteSpace(property.Value.ToString()))
            {
                var imageCropperJson = property.Value.ToString();
                var jImageCropperData = JObject.Parse(imageCropperJson);
                var file = (string)jImageCropperData["src"];

                if (FileHelpers.FileExists(file))
                {
                    propertyTag.Add(
                    new XAttribute(property.Alias, file),
                    new XAttribute("fileName", file.Split('/').Last()),
                    new XAttribute("objectType", ObjectTypes.File),
                    new XAttribute("dependentAsset", file));            // special attribute used to retrieve dependent assets
                }

                jImageCropperData = ConvertMediaUrlToGuid(property.Version, jImageCropperData);

                propertyTag.Value = jImageCropperData.ToString();
            }
        }

        public string Import(XElement propertyTag)
        {
            var result = string.Empty;

            if (!string.IsNullOrWhiteSpace(propertyTag.Value))
            {
                var imageCropperJson = propertyTag.Value;
                var jImageCropperData = JObject.Parse(imageCropperJson);

                result = ConvertGuidToMediaUrl(jImageCropperData).ToString();
            }

            return result;
        }

        public string GetUrl(string propertyData)
        {
            if (string.IsNullOrEmpty(propertyData))
            {
                throw new ArgumentNullException("propertyData");
            }

            var jMedia = JObject.Parse(propertyData);
            return (string)jMedia["src"];
        }

        #region Helpers

        /// <summary>
        /// Converts media urls in the JSON to the corresponding media GUID.
        /// </summary>
        /// <param name="mediaVersionGuid"></param>
        /// <param name="jImageCropperData">Image Cropper Json</param>
        /// <returns>Adjusted Image Cropper Json</returns>
        private JObject ConvertMediaUrlToGuid(Guid mediaVersionGuid, JObject jImageCropperData)
        {
            if (jImageCropperData == null)
            {
                throw new ArgumentNullException("jImageCropperData");
            }

            var file = (string)jImageCropperData["src"];

            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentException("Expected src attribute in ImageCropper json but could not find it");
            }

            if (!string.IsNullOrEmpty(file) && FileHelpers.FileExists(file))
            {
                var filename = file.Split('/').LastOrDefault();
                if (filename != null && filename.Contains("?"))
                {
                    file = file.Remove(file.IndexOf("?", StringComparison.Ordinal));
                }

                // get the media associated with the property
                var mediaVersion = Services.MediaService.GetByVersion(mediaVersionGuid);
                var media = Services.MediaService.GetById(mediaVersion.Id);

                if (media != null)
                {
                    jImageCropperData["src"] = media.Key;
                }
            }

            return jImageCropperData;
        }

        /// <summary>
        /// Converts media GUID in the JSON to the corresponding media GUID
        /// </summary>
        /// <param name="jImageCropperData">Image Cropper Json</param>
        /// <returns>Adjusted Image Cropper Json</returns>
        private JObject ConvertGuidToMediaUrl(JObject jImageCropperData)
        {
            if (jImageCropperData == null)
            {
                throw new ArgumentNullException("jImageCropperData");
            }

            Guid mediaGuid;
            var mediaGuidString = (string)jImageCropperData["src"];

            if (string.IsNullOrEmpty(mediaGuidString) || !Guid.TryParse(mediaGuidString, out mediaGuid))
            {
                throw new ArgumentException("Expected src attribute in ImageCropper json but could not find it");
            }

            if (mediaGuid != Guid.Empty)
            {
                var media = Services.MediaService.GetById(mediaGuid);

                if (media != null)
                {
                    var uploadFieldAlias = GetUploadFieldAlias(media);
                    var mediaJson = media.Properties[uploadFieldAlias].Value.ToString();
                    jImageCropperData["src"] = GetUrl(mediaJson);
                }
            }

            return jImageCropperData;
        }

        /// <summary>
        /// Finds the property alias of the property where files are uploaded
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private string GetUploadFieldAlias(IContentBase node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            var uploadFieldNodes = umbraco.UmbracoSettings._umbracoSettings.SelectNodes("/content/imaging/autoFillImageProperties/uploadField");
            if (uploadFieldNodes == null)
            {
                throw new ConfigurationErrorsException("Expected /content/imaging/autoFillImageProperties/uploadField element");
            }

            foreach (XmlNode uploadFieldNode in uploadFieldNodes)
            {
                if (uploadFieldNode.Attributes != null && uploadFieldNode.Attributes["alias"] != null)
                {
                    var alias = uploadFieldNode.Attributes["alias"].Value;
                    if (node.HasProperty(alias))
                    {
                        return uploadFieldNode.Attributes["alias"].Value;
                    }
                }
            }

            throw new Exception(string.Format("Could not determine uploadField alias for node with id: {0}. Either something went wrong with the Conveyor export/import or the uploadField alias could not be determined from the umbracoSettings.config", node.Id));
        }

        #endregion
    }
}
