using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace QWERTYShop
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        void Session_Start()
        {
            string userIp = Request.UserHostAddress;
            string request = @"http://api.ipstack.com/" + userIp + @"?access_key=00981594f20ffe322488ebd4b9ad9678&fields=city";
            string result = "";
            using (WebClient wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                result = wc.UploadString(request, "");
                result = result.Remove(0, 8);
                result = result.Remove(result.Length - 1, 1);
            }
            Session["CityName"] = result;
        }
    }
}
