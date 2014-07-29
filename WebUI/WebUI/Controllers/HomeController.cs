namespace WebUI.Controllers
{
    using System.Web.Mvc;
    using Umbraco.Web.Mvc;
    
    public class HomeController : SurfaceController
    {
        public PartialViewResult RenderHomePage()
        {
            return PartialView("_Home");
        }
    }
}