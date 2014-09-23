namespace AST.ContentPackage
{
    using System.Web.Mvc;
    using System.Web.Routing;

    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute(
                name: "ExampleMVCBackofficePages",
                url: "App_Plugins/BackOffice/ContentPackage/{action}/{id}",
                defaults: new { controller = "ContentPackage", action = "Index", id = UrlParameter.Optional });
        }
    }
}