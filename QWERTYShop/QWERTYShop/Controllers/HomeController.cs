using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using QWERTYShop.Models;
using System.Threading;
using System.Text;

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

        public ActionResult Cart() //5435:2,12:1
        {
            if (Session["cart"] == null)
            {
                ViewBag.CartMessage = "Ваша корзина пустая";
                ViewBag.CartContainer = "";
                ViewBag.CartPrice = "";
                return View();
            }

            int totalPrice = 0;
            var currCart = Session["cart"].ToString(); //парсинг
            List<CartModels> cartElements = new List<CartModels>();
            string[] idsAndCount;
            if (currCart.Contains(','))
            {
                idsAndCount = currCart.Split(',');
            }
            else
            {
                idsAndCount = new string[1];
                idsAndCount[0] = currCart;
            }
            for (var i = 0; i < idsAndCount.Length; i++)
            {
                var parsedIdsAndCount = idsAndCount[i].Split(':');
                cartElements.Add(new CartModels{Id = long.Parse(parsedIdsAndCount[0]), Count = int.Parse(parsedIdsAndCount[1])});
            }

            for (int i = 0; i < cartElements.Count; i++)
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (NpgsqlCommand command = new NpgsqlCommand($"SELECT name, cost FROM public.cards where id={cartElements[i].Id};", connection))
                    {
                        var reader = command.ExecuteReader();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                cartElements[i].Name = reader.GetString(0);
                                cartElements[i].Cost = reader.GetInt32(1);
                            }
                        }
                    }
                }
            } //получение информации по Id

            ViewBag.CartContainer = cartElements;

            for (int i = 0; i < cartElements.Count; i++) //подсчёт общей суммы
            {
                totalPrice += (cartElements[i].Cost * cartElements[i].Count);
            }
            ViewBag.CartPrice = totalPrice.ToString();
            return View();
        }

        [Route("addtocard/{id}")]
        public ActionResult AddToCart(long? id)
        {
            if (Session["cart"] == null)
            {
                Session["cart"] = id + ":1";
                return RedirectToAction("Cart");
            }
            string[] idsAndCount;
            var cart = Session["cart"].ToString();
            if (cart.Contains(','))
            {
                idsAndCount = cart.Split(',');
            }
            else
            {
                idsAndCount = new string[1];
                idsAndCount[0] = cart;
            }

            for (var i = 0; i < idsAndCount.Length; i++)
            {
                var parsedIdsAndCount = idsAndCount[i].Split(':');
                if (long.Parse(parsedIdsAndCount[0]) == id)
                    return RedirectToAction("Cart"); //выдавать сообщения, типо товар уже в корзине
            }

            var currCart=Session["cart"].ToString();
            currCart += $",{id}:1";
            Session["cart"] = currCart;

            return RedirectToAction("Cart");
        }

        [HttpPost]
        public ActionResult Cart(CartModels model)
        {
            if (model.Method == "/")
            {
                ViewBag.CartMessage = "Ваша корзина пустая";
                ViewBag.CartContainer = "";
                ViewBag.CartPrice = "";
                Session["cart"] = null;
                return View();
            }
            int totalPrice = 0;
            if(Session["cart"]==null)
            {
                ViewBag.CartMessage = "Ваша корзина пустая";
                ViewBag.CartContainer = "";
                ViewBag.CartPrice = "";
                return View();
            }
            var currCart = Session["cart"].ToString(); //парсинг
            List<CartModels> cartElements = new List<CartModels>();
            string[] idsAndCount;
            if (currCart.Contains(','))
            {
                idsAndCount = currCart.Split(',');
            }
            else
            {
                idsAndCount = new string[1];
                idsAndCount[0] = currCart;
            }
            for (var i = 0; i < idsAndCount.Length; i++)
            {
                var parsedIdsAndCount = idsAndCount[i].Split(':');
                var id = long.Parse(parsedIdsAndCount[0]);
                var count = int.Parse(parsedIdsAndCount[1]);
                if (id == model.Id && model.Method=="+") count++;
                if (id == model.Id && model.Method == "-") count--;
                if (count!=0) 
                    cartElements.Add(new CartModels { Id=id, Count=count});
            }

            if(cartElements.Count==0)
            {
                ViewBag.CartMessage = "Ваша корзина пустая";
                ViewBag.CartContainer = "";
                ViewBag.CartPrice = "";
                Session["cart"] = null;
                return View();
            }

            for (int i = 0; i < cartElements.Count; i++)
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (NpgsqlCommand command = new NpgsqlCommand($"SELECT name, cost FROM public.cards where id={cartElements[i].Id};", connection))
                    {
                        var reader = command.ExecuteReader();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                cartElements[i].Name = reader.GetString(0);
                                cartElements[i].Cost = reader.GetInt32(1);
                            }
                        }
                    }
                }
            } //получение информации по Id

            ViewBag.CartContainer = cartElements;

            for (int i = 0; i < cartElements.Count; i++) //подсчёт общей суммы
            {
                totalPrice += (cartElements[i].Cost * cartElements[i].Count);
            }
            ViewBag.CartPrice = totalPrice.ToString();


            StringBuilder str = new StringBuilder();
            for (int i = 0; i < cartElements.Count; i++)
            {
                if (cartElements.Count == 1)
                    str.Append(cartElements[i].Id + ":" + cartElements[i].Count);
                else
                    if (i == cartElements.Count-1)
                        str.Append(cartElements[i].Id + ":" + cartElements[i].Count);
                    else
                        str.Append(cartElements[i].Id + ":" + cartElements[i].Count+",");
            }
            Session["cart"] = str.ToString();
            return View();
        }

        [Authorize]
        public ActionResult PurchaseInfo()
        {
            return View();
        }
    }
}