namespace WebUI.Infrastructure
{
    using umbraco.businesslogic;
    using umbraco.interfaces;

    [Application(Constants.ApplicationAlias, Constants.ApplicationName, "umbraco-custom-tray-icon.gif")]
    public class ContentPackageApplication : IApplication
    {
    }
}