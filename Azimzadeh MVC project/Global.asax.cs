using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Azimzadeh_MVC_project
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var exception = Server.GetLastError();
            var httpException = exception as HttpException;
            if (httpException != null && httpException.GetHttpCode() == 404)
            {
                Response.Clear();
                Server.ClearError();
                Response.TrySkipIisCustomErrors = true;

                IController errorController = new Controllers.HomeController();
                var routeData = new RouteData();
                routeData.Values.Add("controller", "Home");
                routeData.Values.Add("action", "Error404");

                var requestContext = new RequestContext(new HttpContextWrapper(Context), routeData);
                errorController.Execute(requestContext);
            }
        }
    }
}
