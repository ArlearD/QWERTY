using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web.Mail;
using System.Web.Mvc;
using System.Web.WebPages;
using Microsoft.AspNet.Identity;
using Npgsql;
using QWERTYShop.Models;
using MailMessage = System.Net.Mail.MailMessage;

namespace QWERTYShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly string ConnectionString =
            "Server = localhost; Port=5432; Database=postgres; User Id =postgres; Password=1234QWER+";

        public ActionResult Index()
        {
            var Data = new List<CardsModels>();
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("select * from cards order by addedtime desc limit(3);", connection))
                {
                    var reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            var card = new CardsModels();
                            card.Id = reader.GetInt64(0);
                            card.Name = reader.GetString(1);
                            card.Type = reader.GetString(2);
                            card.Image = reader.GetString(4);
                            card.Information = reader.GetString(5);
                            card.Cost = reader.GetInt32(6);
                            Data.Add(card);
                        }
                }
                ViewBag.Cards = Data;
            }

            return View();
        }

        [Route("card/{id}")]
        public ActionResult Card(long? id)
        {
            var card = new CardsModels();
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand($"SELECT * FROM public.cards where id={id};", connection))
                {
                    var reader = command.ExecuteReader();
                    if (reader.HasRows)
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
            var Data = new List<string>();
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT City FROM public.CityList;", connection))
                {
                    var reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                            Data.Add(reader.GetString(0));
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
            var Data = new List<string>();
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT City FROM public.CityList;", connection))
                {
                    var reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                            Data.Add(reader.GetString(0));
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

            var totalPrice = 0;
            var currCart = Session["cart"].ToString(); //парсинг
            var cartElements = new List<CartModels>();
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
                cartElements.Add(new CartModels
                { Id = long.Parse(parsedIdsAndCount[0]), Count = int.Parse(parsedIdsAndCount[1]) });
            }

            for (var i = 0; i < cartElements.Count; i++)
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (var command =
                        new NpgsqlCommand($"SELECT name, cost FROM public.cards where id={cartElements[i].Id};",
                            connection))
                    {
                        var reader = command.ExecuteReader();
                        if (reader.HasRows)
                            while (reader.Read())
                            {
                                cartElements[i].Name = reader.GetString(0);
                                cartElements[i].Cost = reader.GetInt32(1);
                            }
                    }
                }

            ViewBag.CartContainer = cartElements;

            for (var i = 0; i < cartElements.Count; i++) //подсчёт общей суммы
                totalPrice += cartElements[i].Cost * cartElements[i].Count;
            ViewBag.CartPrice = totalPrice.ToString();
            Session["Price"] = totalPrice;
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

            var currCart = Session["cart"].ToString();
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

            if (Session["cart"] == null)
            {
                ViewBag.CartMessage = "Ваша корзина пустая";
                ViewBag.CartContainer = "";
                ViewBag.CartPrice = "";
                return View();
            }

            var totalPrice = 0;
            var currCart = Session["cart"].ToString(); //парсинг
            var cartElements = new List<CartModels>();
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
                if (id == model.Id && model.Method == "+") count++;
                if (id == model.Id && model.Method == "-") count--;
                if (count != 0)
                    cartElements.Add(new CartModels { Id = id, Count = count });
            }

            if (cartElements.Count == 0)
            {
                ViewBag.CartMessage = "Ваша корзина пустая";
                ViewBag.CartContainer = "";
                ViewBag.CartPrice = "";
                Session["cart"] = null;
                return View();
            }

            for (var i = 0; i < cartElements.Count; i++)
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (var command =
                        new NpgsqlCommand($"SELECT name, cost FROM public.cards where id={cartElements[i].Id};",
                            connection))
                    {
                        var reader = command.ExecuteReader();
                        if (reader.HasRows)
                            while (reader.Read())
                            {
                                cartElements[i].Name = reader.GetString(0);
                                cartElements[i].Cost = reader.GetInt32(1);
                            }
                    }
                }

            ViewBag.CartContainer = cartElements;

            for (var i = 0; i < cartElements.Count; i++) //подсчёт общей суммы
                totalPrice += cartElements[i].Cost * cartElements[i].Count;
            ViewBag.CartPrice = totalPrice.ToString();
            Session["Price"] = totalPrice;

            var str = new StringBuilder();
            for (var i = 0; i < cartElements.Count; i++)
                if (cartElements.Count == 1)
                    str.Append(cartElements[i].Id + ":" + cartElements[i].Count);
                else if (i == cartElements.Count - 1)
                    str.Append(cartElements[i].Id + ":" + cartElements[i].Count);
                else
                    str.Append(cartElements[i].Id + ":" + cartElements[i].Count + ",");
            Session["cart"] = str.ToString();
            return View();
        }

        [Authorize]
        public ActionResult PurchaseInfo()
        {
            string isAvailableForPickup = "False";
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command =
                    new NpgsqlCommand($"SELECT isavailableforpickup FROM public.citylist where city='{Session["CityName"]}';",
                        connection))
                {
                    var reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            isAvailableForPickup = reader.GetBoolean(0).ToString();
                        }
                }
            }
            ViewBag.IsAvailableForPickup = isAvailableForPickup;
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult PurchaseInfo(PurchaseModels model)
        {
            Session["Delivery"] = model.Method;
            return RedirectToAction("PurchaseData");
        }

        public ActionResult Success()
        {
            bool isPaid = Session["Paid"] == "true";
            ViewBag.Id = GetNextIdOfPurchases();
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                NpgsqlCommand command =
                    new NpgsqlCommand(
                        "INSERT INTO public.currentpurchase(id,purchase, condition, paid, typeofdelivery, mail) values(@i,@p, @c, @paid, @t, @m)", connection);

                command.Parameters.AddWithValue("i", GetNextIdOfPurchases());
                command.Parameters.AddWithValue("p", Session["Cart"]);
                command.Parameters.AddWithValue("c", "handles");
                command.Parameters.AddWithValue("paid", isPaid);
                command.Parameters.AddWithValue("t", Session["Delivery"].ToString());
                command.Parameters.AddWithValue("m", Session["Mail"].ToString());
                command.ExecuteNonQuery();

                NpgsqlCommand command2 = new NpgsqlCommand("INSERT INTO public.finishedpurchases(id) " +
                                                           "values(@i)", connection);
                command2.Parameters.AddWithValue("i", GetNextIdOfPurchases());
                command2.ExecuteNonQuery();
                connection.Close();
            }

            string information = "";
            if (Session["Delivery"].ToString() == "Самовывоз" && Session["Paid"] == "true")
            {
                information = $"Выбранный Вами тип получения товара - самовывоз, приходите в пункт самовывоза " +
                              $"{Session["PickupDate"].ToString().Split(' ')[0]}, часы работы:{Session["PickupDate"].ToString().Split(' ')[1]}";
            }
            else if (Session["Delivery"].ToString() == "Самовывоз")
            {
                information = $"Выбранный Вами тип получения товара - самовывоз, приходите в пункт самовывоза " +
                              $"{Session["PickupDate"].ToString().Split(' ')[0]}, часы работы:{Session["PickupDate"].ToString().Split(' ')[1]}" +
                              $" Напоминаем, что сумма Вашего заказа составляет {Session["Price"]}";
            }
            else if (Session["Delivery"].ToString() == "Доставка до квартиры" && Session["Paid"] == "true")
            {
                information = $"Выбранный Вами тип получения товара - доставка до квартиры. Ожидайте нашего курьера " +
                              $"{Session["DeliveryDate"].ToString().Split(' ')[0]}, время доставки: {Session["DeliveryTime"].ToString()}";
            }
            else
            {
                information = $"Выбранный Вами тип получения товара - доставка до квартиры. Ожидайте нашего курьера" +
                              $"{Session["DeliveryDate"].ToString().Split(' ')[0]}, время доставки:{Session["DeliveryTime"].ToString()}" +
                              $" Напоминаем, что сумма Вашего заказа составляет {Session["Price"]}";
            }

            information += $" Номер вашего заказа: {ViewBag.Id}";

            SmtpClient client = new SmtpClient();
            client.Host = "smtp.gmail.com";
            client.Port = 587;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential("qqqwertyshop@gmail.com", "1234QWER+");
            client.Send("qqqwertyshop@gmail.com", Session["PurchaseMail"].ToString(), "Заказ успешно оформлен!", information);

            return View();
        }

        public ActionResult Payment()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Payment(PaymentModels model)
        {
            if (ModelState.IsValid)
            {
                Session["Paid"] = "true";
                return Redirect("success");
            }

            return Redirect("payment");
        }

        public ActionResult PurchaseData()
        {
            var currentDate = DateTime.Today.Date;
            List<string> availableDatesOfPickup = new List<string>();
            for (int i = 1; i <= 7; i++)
            {
                availableDatesOfPickup.Add(currentDate.AddDays(i).ToShortDateString().Split(' ')[0] + " 9:00-20:00");
            }

            ViewBag.AvailableDatesOfPickup = availableDatesOfPickup;

            List<string> availableDatesOfDelivery = new List<string>();
            List<string> availableTimesOfDelivery = new List<string>();
            for (int i = 1; i <= 14; i++)
            {
                availableDatesOfDelivery.Add(currentDate.AddDays(i).ToShortDateString().Split(' ')[0]);
            }

            availableTimesOfDelivery.Add("10:00-14:00");
            availableTimesOfDelivery.Add("14:00-18:00");
            availableTimesOfDelivery.Add("18:00-21:00");

            ViewBag.AvailableTimesOfDelivery = availableTimesOfDelivery;
            ViewBag.AvailableDatesOfDelivery = availableDatesOfDelivery;
            return View();
        }

        [HttpPost]
        public ActionResult PurchaseData(PurchaseModels model)
        {
            var currentDate = DateTime.Today.Date;
            List<string> availableDatesOfPickup = new List<string>();
            for (int i = 1; i <= 7; i++)
            {
                availableDatesOfPickup.Add(currentDate.AddDays(i).ToShortDateString().Split(' ')[0] + " 9:00-20:00");
            }

            ViewBag.AvailableDatesOfPickup = availableDatesOfPickup;

            List<string> availableDatesOfDelivery = new List<string>();
            List<string> availableTimesOfDelivery = new List<string>();
            for (int i = 1; i <= 14; i++)
            {
                availableDatesOfDelivery.Add(currentDate.AddDays(i).ToShortDateString().Split(' ')[0]);
            }

            availableTimesOfDelivery.Add("10:00-14:00");
            availableTimesOfDelivery.Add("14:00-18:00");
            availableTimesOfDelivery.Add("18:00-21:00");

            ViewBag.AvailableTimesOfDelivery = availableTimesOfDelivery;
            ViewBag.AvailableDatesOfDelivery = availableDatesOfDelivery;

            GetInformation(model);

            Session["Mail"] = model.Mail;

            if (ModelState.IsValid && model.Payment == "Оплата онлайн")
            {
                return Redirect("payment");
            }

            if (model.Payment == "Оплата онлайн")
            {
                ViewBag.Message = "Введите поля корректно!";
                return View();
            }

            if (ModelState.IsValid)
            {
                return Redirect("success");
            }

            ViewBag.Message = "Введите поля корректно!";
            return View();
        }

        private void GetInformation(PurchaseModels model)
        {
            Session["PurchaseMail"] = model.Mail;
            Session["PickupDate"] = model.PickupDate;
            Session["DeliveryDate"] = model.DeliveryDate;
            Session["DeliveryTime"] = model.DeliveryTime;
        }

        private long GetNextIdOfPurchases()
        {
            long currMax = 0;
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("select id FROM public.finishedpurchases WHERE ID=(SELECT MAX(id) FROM finishedpurchases);", connection))
                {
                    var reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            currMax = reader.GetInt32(0);
                        }
                }
                connection.Close();
            }
            return currMax + 1;
        }
    }
}