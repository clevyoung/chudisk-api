using System.Web.Mvc;

namespace WebDiskApplication.Areas.WebDisk
{
    public class WebDiskAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "WebDisk";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "WebDisk_default",
                "WebDisk/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}