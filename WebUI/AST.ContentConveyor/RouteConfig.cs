namespace AST.ContentConveyor
{
    using System.Web.Mvc;
    using System.Web.Routing;

    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute(
                name: "ASTContentConveyorBackofficePages",
                url: "App_Plugins/BackOffice/ContentConveyor/{action}/{id}",
                defaults: new { controller = "ContentConveyor", action = "Index", id = UrlParameter.Optional });
        }
    }
}