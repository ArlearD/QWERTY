using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace QWERTYShop.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            string userIp = Request.UserHostAddress;
            string request= @"http://api.ipstack.com/"+userIp+@"?access_key=00981594f20ffe322488ebd4b9ad9678&fields=city";
            string result = "";
            using (WebClient wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                result = wc.UploadString(request, "");
                result=result.Remove(0, 8);
                result = result.Remove(result.Length - 1, 1);
            }

            @ViewBag.City = result;
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}