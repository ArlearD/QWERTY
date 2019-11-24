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
            Connection.ConnectionString;

        public ActionResult Index()
        {
            var Data = new List<CardsModels>();
            var Caterogies = new List<CategoryModels>();
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command =
                    new NpgsqlCommand("select * from cards order by addedtime desc limit(3);", connection))
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

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("select distinct type from cards;", connection))
                {
                    var reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            Caterogies.Add(new CategoryModels { Type = reader.GetString(0) });
                        }

                    ViewBag.Categories = Caterogies;
                }
            }
            return View();
        }

        [Route("card/{id}")]
        public ActionResult Card(long id)
        {
            var card = GetOutputForCard(id);
            var IdsToOutput = GetAdditionalIdProducts(id);
            if (IdsToOutput.Count != 0)
                CreateCardToBuyWith(IdsToOutput);
            else ViewBag.PurchasedId = new List<CardsModels>();

            ViewBag.AverageMark = GetAverageMark(id);
            return View(card);
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

        [Route("card/{id}/commentaries")]
        public ActionResult Commentaries()
        {
            GetCommentaries(long.Parse(Request.Url.ToString().Split('/')[4]));
            return View();
        }

        [Route("card/{id}/commentaries")]
        [HttpPost]
        public ActionResult Commentaries(CommentariesModels model)
        {
            if (ModelState.IsValid)
            {
                model.Time = DateTime.Today;
                using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    NpgsqlCommand command =
                        new NpgsqlCommand(
                            $"INSERT INTO commentaries values('{model.UserName}', '{model.Comment}', '{model.Time}', {model.Id}, {model.Mark} )", connection);
                    command.ExecuteNonQuery();
                    connection.Close();
                    connection.Open();
                    NpgsqlCommand cmd = new NpgsqlCommand($"update cards set averagemark={GetAverageMark(model.Id)} where id={model.Id};", connection);
                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
            }
            GetCommentaries(long.Parse(Request.Url.ToString().Split('/')[4]));

            return View();
        }

        private void GetCommentaries(long id)
        {
            List<CommentariesModels> data = new List<CommentariesModels>();
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command =
                    new NpgsqlCommand($"SELECT * FROM commentaries where id={id};",
                        connection))
                {
                    var reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            data.Add(new CommentariesModels
                            {
                                UserName = reader.GetString(0),
                                Comment = reader.GetString(1),
                                Time = reader.GetDateTime(2),
                                Id = reader.GetInt64(3),
                                Mark = reader.GetInt32(4)
                            });
                        }
                }
            }
            ViewBag.Commentaries = data;
        }

        private float GetAverageMark(long id)
        {
            float avg = -1;
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command =
                    new NpgsqlCommand($"Select avg(mark) from(SELECT mark FROM commentaries where id={id}) as a;",
                        connection))
                {
                    var reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                                avg = reader.GetFloat(0);
                        }
                }
            }
            return avg;
        }

        private List<long> GetAdditionalIdProducts(long id)
        {
            List<PurchasedProductsModels> data = new List<PurchasedProductsModels>();
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command =
                    new NpgsqlCommand($"Select id, purchasedproducts from finishedpurchases",
                        connection))
                {
                    var reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(1))
                                data.Add(new PurchasedProductsModels { Id = reader.GetInt64(0), PurchasedProducts = reader.GetString(1) });
                        }
                }
            }

            List<long> idsToOutput = new List<long>();

            foreach (var element in data)
            {
                var splited = element.PurchasedProducts.Split(',');
                if (splited.Contains(id.ToString())) //?
                {
                    for (int i = 0; i < splited.Length; i++)
                    {
                        idsToOutput.Add(long.Parse(splited[i]));
                    }
                }
            }

            idsToOutput = idsToOutput.Distinct().Where(x => x != id).ToList();
            return idsToOutput;
        }

        private void CreateCardToBuyWith(List<long> list)
        {
            List<CardsModels> data = new List<CardsModels>();

            string commandStr = "select name, id, image, cost from cards where ";

            for (int i = 0; i < list.Count; i++)
            {
                if (i == list.Count - 1)
                {
                    commandStr += $"id={list[i]}";
                }
                else
                {
                    commandStr += $"id={list[i]} or ";
                }
            }

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command =
                    new NpgsqlCommand(commandStr, connection))
                {
                    var reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            data.Add(new CardsModels
                            {
                                Name = reader.GetString(0),
                                Id = reader.GetInt64(1),
                                Image = reader.GetString(2),
                                Cost = reader.GetInt32(3)
                            });
                        }
                }

                ViewBag.PurchasedId = data;
            }
        }

        private CardsModels GetOutputForCard(long id)
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
            }

            string specialName = new ManagmentController().GetSpecialNameUsingType(card.Type); //интересно что будет
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                string[] namesOfProperties = null;
                var properties = new List<string>();
                int numOfColumns = 0;

                connection.OpenAsync();
                NpgsqlCommand cmd = new NpgsqlCommand($"SELECT count(*) FROM information_schema.columns where table_name='{specialName}'", connection);
                var reader = cmd.ExecuteReaderAsync();
                while (reader.Result.Read())
                {
                    numOfColumns = reader.Result.GetInt32(0);
                }
                connection.Close();

                connection.OpenAsync();
                cmd = new NpgsqlCommand($"select * from {specialName} where id={id}", connection);
                reader = cmd.ExecuteReaderAsync();
                while (reader.Result.Read())
                {
                    for (int i = 1; i < numOfColumns; i++)
                        properties.Add(reader.Result.GetString(i));

                }
                connection.Close();
                connection.OpenAsync();
                cmd = new NpgsqlCommand($"select properties from types where type='{card.Type}'", connection);
                reader = cmd.ExecuteReaderAsync();
                while (reader.Result.Read())
                {
                    namesOfProperties = (string[])reader.Result.GetValue(0);
                }
                connection.Close();
                ViewBag.NamesOfProperties = namesOfProperties;
                ViewBag.Properties = properties;
            }
            return card;
        }
    }
}