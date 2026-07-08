using System.Web.Mvc;

namespace Azimzadeh_MVC_project.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Admin_Login",
                "Admin/Login",
                new { controller = "Dashboard", action = "Login" },
                namespaces: new[] { "Azimzadeh_MVC_project.Areas.Admin.Controllers" }
            );

            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                new { controller = "Dashboard", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "Azimzadeh_MVC_project.Areas.Admin.Controllers" }
            );
        }
    }
}
