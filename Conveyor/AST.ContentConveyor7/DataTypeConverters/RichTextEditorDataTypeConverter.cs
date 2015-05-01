using System.Configuration;
using Umbraco.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using AST.ContentConveyor7.Enums;
using Umbraco.Core.Models;

namespace AST.ContentConveyor7.DataTypeConverters
{


    public class RichTextEditorDataTypeConverter : BaseContentManagement, IDataTypeConverter
    {
        public void Export(Property property, XElement propertyTag, Dictionary<int, ObjectTypes> dependantNodes)
        {
            if (property.Value != null && !string.IsNullOrWhiteSpace(property.Value.ToString()))
            {
                var input = property.Value.ToString();

                // find all the internal links that refers to the ID of the page.
                input = ConvertInternalLinkToGuid(dependantNodes, input);

                // find all the images and change the url to guid
                input = ConvertMediaUrlToGuidOnImageTag(dependantNodes, input);

                input = ConvertMediaUrlToGuidOnAnchorTag(dependantNodes, input);

                propertyTag.Value = input;
            }
        }

        public string Import(XElement propertyTag)
        {
            var result = string.Empty;

            if (!string.IsNullOrWhiteSpace(propertyTag.Value))
            {
                var input = propertyTag.Value;

                input = this.ConvertGuidToInternalLink(input);

                input = this.ConvertGuidToMediaUrlOnImageTag(input);
                
                input = this.ConvertGuidToMediaUrlOnAnchorTag(input);

                result = input;
            }

            return result;
        }

        #region Export Helpers

        private string ConvertInternalLinkToGuid(Dictionary<int, ObjectTypes> dependantNodes, string input)
        {
            var matchesUrls = Regex.Matches(input, @"<a href=""/{localLink:(\d+)}""");

            if (matchesUrls.Count > 0)
            {
                foreach (Match match in matchesUrls)
                {
                    int id;
                    var node = match.Groups[1].Value;

                    if (int.TryParse(node, out id))
                    {
                        var content = Services.ContentService.GetById(id);
                        input = input.Replace(match.Value, @"<a href=""/{localLink:" + content.Key + @"}"" ");
                    }
                }
            }

            return input;
        }

        private string ConvertMediaUrlToGuidOnImageTag(Dictionary<int, ObjectTypes> dependantNodes, string input)
        {
            var matchesImages = Regex.Matches(input, @"<img(?<attr1>.*?)src=""(?<url>/media/.*?)""(?<attr2>.*?)rel=""(?<rel>\d+)"".*/>");

            if (matchesImages.Count > 0)
            {
                foreach (Match match in matchesImages)
                {
                    int mediaId;
                    var mediaIdString = match.Groups["rel"].Value;
                    if (!Int32.TryParse(mediaIdString, out mediaId))
                    {
                        throw new Exception("RTE media link was not properly linked to a media item via the rel attribute. rel should point to the media id.");
                    }

                    var media = Services.MediaService.GetById(mediaId);

                    if (media == null)
                    {
                        throw new Exception(string.Format("Could not find media by id, {0}", mediaId));
                    }

                    var outputLink = string.Format(@"<img{0}src=""{1}""{2} />", match.Groups["attr1"].Value, media.Key, match.Groups["attr2"].Value);
                    input = input.Replace(match.Value, outputLink);

                    if (!dependantNodes.ContainsKey(media.Id))
                    {
                        dependantNodes.Add(media.Id, ObjectTypes.Media);
                    }
                }
            }

            return input;
        }

        private string ConvertMediaUrlToGuidOnAnchorTag(Dictionary<int, ObjectTypes> dependantNodes, string input)
        {
            var matchesImages = Regex.Matches(input, @"<a(?<attr1>.*?)href=""(?<url>/media/.*?)""(?<attr2>.*?)>");

            if (matchesImages.Count > 0)
            {
                foreach (Match match in matchesImages)
                {
                    var mediaUrl = match.Groups["url"].Value;

                    var media = Services.MediaService.GetMediaByPath(mediaUrl);

                    if (media == null)
                    {
                        throw new Exception(string.Format("Could not find media by url, {0}", mediaUrl));
                    }

                    var outputLink = string.Format(@"<img{0}src=""{1}""{2} />", match.Groups["attr1"].Value, media.Key, match.Groups["attr2"].Value);
                    input = input.Replace(match.Value, outputLink);

                    if (!dependantNodes.ContainsKey(media.Id))
                    {
                        dependantNodes.Add(media.Id, ObjectTypes.Media);
                    }
                }
            }

            return input;
        }

        #endregion

        #region Import Helpers

        private string ConvertGuidToInternalLink(string input)
        {
            // find all the internal links that refers to guid of the page.
            var matchesUrls = Regex.Matches(
                input,
                @"<a href=""/{localLink:(\b[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}\b)}",
                RegexOptions.IgnoreCase);

            foreach (Match match in matchesUrls)
            {
                var guid = new Guid(match.Groups[1].Value);
                var content = Services.ContentService.GetById(guid);

                if (content != null)
                {
                    input = input.Replace(match.Value, @"<a href=""/{localLink:" + content.Id + @"}"" ");
                }
            }

            return input;
        }

        private string ConvertGuidToMediaUrlOnImageTag(string input)
        {
            var matchesImages = Regex.Matches(input, @"<img(?<attr1>.*?)src=""(?<guid>\b[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}\b)""(?<attr2>.*?)/>", RegexOptions.IgnoreCase);

            if (matchesImages.Count > 0)
            {
                foreach (Match match in matchesImages)
                {
                    var guid = match.Groups["guid"].Value;

                    var media = Services.MediaService.GetById(new Guid(guid));

                    if (media != null)
                    {
                        // import media url. Need to leverage the DataTypeConverter for this (ex. media data is stored as JSON for ImageCropper)
                        var uploadFieldAlias = GetUploadFieldAlias(media);
                        var uploadPropertyIdx = media.Properties.IndexOfKey(uploadFieldAlias);
                        var uploadPropertyType = media.PropertyTypes.ElementAt(uploadPropertyIdx);
                        var converterTypeKey = SpecialDataTypes[uploadPropertyType.DataTypeId];
                        var dataTypeConverter = GetUploadDataTypeConverterInterface(converterTypeKey);

                        var mediaUrl = dataTypeConverter.GetUrl(media.Properties[uploadFieldAlias].Value.ToString());

                        var outputLink = string.Format(@"<img{0}src=""{1}""{2}rel=""{3}"" />",
                            match.Groups["attr1"].Value,
                            mediaUrl,
                            match.Groups["attr2"].Value,
                            media.Id);
                        input = input.Replace(match.Value, outputLink);
                    }
                }
            }

            return input;
        }

        private string ConvertGuidToMediaUrlOnAnchorTag(string input)
        {
            var matchesImages = Regex.Matches(input, @"<a href=""(?<guid>\b[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}\b)"">", RegexOptions.IgnoreCase);

            if (matchesImages.Count > 0)
            {
                foreach (Match match in matchesImages)
                {
                    var guid = match.Groups["guid"].Value;

                    var media = Services.MediaService.GetById(new Guid(guid));

                    if (media != null)
                    {
                        // import media url. Need to leverage the DataTypeConverter for this (ex. media data is stored as JSON for ImageCropper)
                        var uploadFieldAlias = GetUploadFieldAlias(media);
                        var uploadPropertyIdx = media.Properties.IndexOfKey(uploadFieldAlias);
                        var uploadPropertyType = media.PropertyTypes.ElementAt(uploadPropertyIdx);
                        var converterTypeKey = SpecialDataTypes[uploadPropertyType.DataTypeId];
                        var dataTypeConverter = GetUploadDataTypeConverterInterface(converterTypeKey);

                        var mediaUrl = dataTypeConverter.GetUrl(media.Properties[uploadFieldAlias].Value.ToString());

                        var outputLink = string.Format(@"<a href=""{0}""rel=""{1}"">", mediaUrl, media.Id);
                        input = input.Replace(match.Value, outputLink);
                    }
                }
            }

            return input;
        }

        #endregion

        #region Helpers

        private IEnumerable<string> GetUploadFieldAliases()
        {
            var uploadFields = UmbracoConfig.For.UmbracoSettings().Content.ImageAutoFillProperties.ToList();
            if (uploadFields == null || uploadFields.All(f => string.IsNullOrEmpty(f.Alias)))
            {
                throw new ConfigurationErrorsException("Expected /content/imaging/autoFillImageProperties/uploadField alias attribute");
            }

            return uploadFields.Where(f => !string.IsNullOrEmpty(f.Alias)).Select(f => f.Alias);
        }

        private string GetUploadFieldAlias(IContentBase node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            var uploadFields = GetUploadFieldAliases().ToList();

            if (!uploadFields.Any(f => !string.IsNullOrEmpty(f) && node.HasProperty(f)))
            {
                throw new Exception(string.Format("Could not determine uploadField alias for node with id: {0}", node.Id));
            }

            return uploadFields.First(f => !string.IsNullOrEmpty(f) && node.HasProperty(f));
        }

        #endregion
    }
}