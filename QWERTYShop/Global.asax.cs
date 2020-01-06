using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Npgsql;
using QWERTYShop.Controllers;
using QWERTYShop.Models;
using StackExchange.Profiling;

namespace QWERTYShop
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            MiniProfiler.Configure(new MiniProfilerOptions());
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_BeginRequest()
        {
            MiniProfiler.StartNew();
        }

        protected void Application_EndRequest()
        {
            MiniProfiler.Current?.Stop();
        }

        void Session_Start()
        {
            Session["Categories"] = new List<CategoryModels>();
            var categories = new List<CategoryModels>();
            using (var connection = new NpgsqlConnection(Connection.ConnectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("select distinct type from cards;", connection))
                {
                    var reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            categories.Add(new CategoryModels { Type = reader.GetString(0) });
                        }

                    Session["Categories"] = categories;
                }
            }


            string userIp = Request.UserHostAddress;
            string request = @"http://api.ipstack.com/" + userIp + @"?access_key=00981594f20ffe322488ebd4b9ad9678&fields=city";
            string result = "";
            string trans = "";
            StringBuilder builder = new StringBuilder();
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                result = wc.UploadString(request, "");
                result = result.Remove(0, 8);
                result = result.Remove(result.Length - 1, 1);
                trans = result;
                result = wc.UploadString($"https://translate.yandex.net/api/v1.5/tr.json/translate?key=trnsl.1.1.20191031T190546Z.cfd7a586341eac4f.41897527629cb4cdc1146a5d3018b00547d5cef4&text={trans}&lang=ru", "");
                result = result.Split(':')[3];
                for (int i = 0; i < result.Length; i++)
                {
                    if (!(result[i] == '[' || result[i] == ']' || result[i] == '{' || result[i] == '}' ||
                          result[i] == '"' || result[i] == '\'' || result[i] == '/' || result[i] == '\\'))
                        builder.Append(result[i]);
                }
            }
            Session["CityName"] = builder.ToString();
        }
    }
}
