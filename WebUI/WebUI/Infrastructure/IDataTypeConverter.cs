namespace WebUI.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;
    using Ionic.Zip;
    using Umbraco.Core.Models;
    using Umbraco.Core.Services;

    public interface IDataTypeConverter
    {
        void Export(Property property, XElement propertyTag, Dictionary<Guid, UmbracoObjectTypes> dependantNodes);

        string Import(XElement propertyTag);
    }
}
