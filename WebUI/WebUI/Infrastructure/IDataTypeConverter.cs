namespace WebUI.Infrastructure
{
    using System.Xml.Linq;
    using Umbraco.Core.Models;
    using Umbraco.Core.Services;

    public interface IDataTypeConverter
    {
        void Export(Property property, ServiceContext services, XElement propertyTag);

        void Import();
    }
}
