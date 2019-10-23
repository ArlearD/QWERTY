using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Npgsql;

namespace WebApplication7.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var connectionString= "Server = localhost; Port=5432; Database=postgres; User Id =postgres; Password=1234QWER+";
            List<string> Data = new List<string>();
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM public.product", connection);
                NpgsqlDataReader dataReader = command.ExecuteReader();
                Data = new List<string>();
                for (int i = 0; dataReader.Read(); i++)
                {
                    Data.Add(dataReader[0].ToString() + ";" + dataReader[1].ToString());
                }
                connection.Close();
            }
            ViewBag.Data = Data;
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