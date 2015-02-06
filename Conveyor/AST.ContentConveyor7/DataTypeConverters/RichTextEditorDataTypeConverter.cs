namespace AST.ContentConveyor7.DataTypeConverters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;

    using AST.ContentConveyor7;
    using AST.ContentConveyor7.Enums;

    using Umbraco.Core.Models;

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

                        ////if (!dependantNodes.ContainsKey(content.Id))
                        ////{
                        ////    // at the moment there is no support for object types document. 
                        ////    dependantNodes.Add(id, ObjectTypes.Document);
                        ////}
                    }
                }
            }

            return input;
        }

        private string ConvertMediaUrlToGuidOnImageTag(Dictionary<int, ObjectTypes> dependantNodes, string input)
        {
            var matchesImages = Regex.Matches(input, @"<img(?<attr1>.*?)src=""(?<url>/media/.*?)""(?<attr2>.*?)/>");

            if (matchesImages.Count > 0)
            {
                foreach (Match match in matchesImages)
                {
                    var url = match.Groups["url"].Value;

                    var filename = url.Split('/').LastOrDefault();
                    if (filename != null && filename.Contains("?"))
                    {
                        url = url.Remove(url.IndexOf("?", StringComparison.Ordinal));
                    }

                    var media = Services.MediaService.GetMediaByPath(url);

                    if (media != null)
                    {
                        var outputLink = string.Format(@"<img{0}src=""{1}""{2} />", match.Groups["attr1"].Value, media.Key, match.Groups["attr2"].Value);
                        input = input.Replace(match.Value, outputLink);

                        if (!dependantNodes.ContainsKey(media.Id))
                        {
                            dependantNodes.Add(media.Id, ObjectTypes.Media);
                        }
                    }
                }
            }

            return input;
        }

        private string ConvertMediaUrlToGuidOnAnchorTag(Dictionary<int, ObjectTypes> dependantNodes, string input)
        {
            var matchesImages = Regex.Matches(input, @"<a href=""(/media/.*)"">");

            if (matchesImages.Count > 0)
            {
                foreach (Match match in matchesImages)
                {
                    var url = match.Groups[1].Value;

                    var media = Services.MediaService.GetMediaByPath(url);

                    if (media != null)
                    {
                        var outputLink = string.Format(@"<a href=""{0}"">",  media.Key);
                        input = input.Replace(match.Value, outputLink);

                        if (!dependantNodes.ContainsKey(media.Id))
                        {
                            dependantNodes.Add(media.Id, ObjectTypes.Media);
                        }
                    }
                }
            }

            return input;
        }

        private string ConvertGuidToMediaUrlOnAnchorTag(string input)
        {
            var matchesImages = Regex.Matches(input, @"<a href=""(\b[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}\b)"">", RegexOptions.IgnoreCase);

            if (matchesImages.Count > 0)
            {
                foreach (Match match in matchesImages)
                {
                    var guid = match.Groups[1].Value;

                    var media = Services.MediaService.GetById(new Guid(guid));

                    if (media != null)
                    {
                        var outputLink = string.Format(@"<a href=""{0}"">", media.Properties["umbracoFile"].Value);
                        input = input.Replace(match.Value, outputLink);
                    }
                }
            }

            return input;
        }

        private string ConvertGuidToMediaUrlOnImageTag(string input)
        {
            var matchesImages = Regex.Matches(input, @"<img(?<attr1>.*?)src=""(\b[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}\b)""(?<attr2>.*?)/>", RegexOptions.IgnoreCase);

            if (matchesImages.Count > 0)
            {
                foreach (Match match in matchesImages)
                {
                    var guid = match.Groups[1].Value;

                    var media = Services.MediaService.GetById(new Guid(guid));

                    if (media != null)
                    {
                        var outputLink = string.Format(
                            @"<img{0}src=""{1}""{2} />",
                            match.Groups["attr1"].Value,
                            media.Properties["umbracoFile"].Value,
                            match.Groups["attr2"].Value);
                        input = input.Replace(match.Value, outputLink);
                    }
                }
            }

            return input;
        }

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
    }
}