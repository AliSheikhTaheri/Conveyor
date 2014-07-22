﻿namespace WebUI.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;
    using Umbraco.Core.Models;

    public class MediaPickerDataTypeConverter : BaseContentManagement, IDataTypeConverter
    {
        public void Export(Property property, XElement propertyTag, Dictionary<Guid, UmbracoObjectTypes> dependantNodes)
        {
            if (property.Value != null && !string.IsNullOrWhiteSpace(property.Value.ToString()))
            {
                var media = Services.MediaService.GetById((int)property.Value);
                propertyTag.Value = media.Key.ToString();

                if (!dependantNodes.ContainsKey(media.Key))
                {
                    dependantNodes.Add(media.Key, UmbracoObjectTypes.Media);
                }
            }
        }

        public string Import(XElement propertyTag)
        {
            var guid = new Guid(propertyTag.Value);
            var id = Services.MediaService.GetById(guid);
            return id.ToString(); 
        }
    }
}