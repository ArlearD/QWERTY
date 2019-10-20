using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using QWERTYShop.Models;

namespace QWERTYShop.Controllers
{
    public class HomeController : Controller
    {
        string ConnectionString = "Server = localhost; Port=5432; Database=postgres; User Id =postgres; Password=1234QWER+";
        public ActionResult Index()
        {
            List<CardsModels> Data = new List<CardsModels>();
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM public.cards;", connection))
                {
                    var reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            CardsModels card = new CardsModels();
                            card.Id = reader.GetInt64(0);
                            card.Name = reader.GetString(1);
                            card.Type = reader.GetString(2);
                            card.Image = reader.GetString(4);
                            card.Information = reader.GetString(5);
                            card.Cost = reader.GetInt32(6);
                            Data.Add(card);
                        }
                    }
                }
                ViewBag.Cards = Data;
            }
            return View();
        }

        [Route("card/{id}")]
        public ActionResult Card(long? id)
        {
            CardsModels card = new CardsModels();
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand($"SELECT * FROM public.cards where id={id};", connection))
                {
                    var reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            card.Id = reader.GetInt64(0);
                            card.Name = reader.GetString(1);
                            card.Type = reader.GetString(2);
                            card.Image = reader.GetString(4);
                            card.Information = reader.GetString(5);
                            card.Cost = reader.GetInt32(6);
                        }
                    }
                }
                return View(card);
            }
        }

        public ActionResult Card()
        {
            return Redirect("/home");
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
        
        [HttpGet]
        public ActionResult ChangeCity()
        {
            List<string> Data = new List<string>();
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand("SELECT City FROM public.CityList;", connection))
                {
                    var reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Data.Add(reader.GetString(0));
                        }
                    }
                }
                ViewBag.CityList = Data;
                return View();
            }
        }

        [HttpPost]
        public ActionResult ChangeCity(CityModels model)
        {
            Session["CityName"] = model.City;
            ViewBag.ChangeCitySuccess = "Успешно!";
            List<string> Data = new List<string>();
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand("SELECT City FROM public.CityList;", connection))
                {
                    var reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Data.Add(reader.GetString(0));
                        }
                    }
                }
                ViewBag.CityList = Data;
                return View();
            }
        }
    }
}