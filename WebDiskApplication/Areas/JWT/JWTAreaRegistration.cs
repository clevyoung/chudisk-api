using System.Web.Mvc;

namespace WebDiskApplication.Areas.JWT
{
    public class JWTAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "JWT";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "JWT_default",
                "JWT/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}