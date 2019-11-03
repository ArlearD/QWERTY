using Npgsql;
using QWERTYShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace QWERTYShop.Controllers
{
    [Authorize(Roles = "admin")]
    public class ManagmentController : Controller
    {
        private string ConnectionString = Connection.ConnectionString;
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AddNewCity()
        {
            GetViewOfCity();
            return View();
        }

        [HttpPost]
        public ActionResult AddNewCity(AddNewCityModels model)
        {
            if (ModelState.IsValid)
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    NpgsqlCommand command = new NpgsqlCommand("INSERT INTO public.citylist(City, IsAvailableForPickup, CostForDelivery) VALUES (@c, @i, @cfd)", connection);
                    command.Parameters.AddWithValue("c", model.City);
                    command.Parameters.AddWithValue("i", model.IsAvailableForPickup);
                    command.Parameters.AddWithValue("cfd", model.CostForDelivery);
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                @ViewBag.AddNewCitySuccess = "Успешно!";
            }
            else
            {
                @ViewBag.AddNewCitySuccess = "Что-то пошло не так, попробуйте снова!";
            }
            GetViewOfCity();
            return View();
        }

        public ActionResult AddNewCard()
        {
            GetTypes();
            return View();
        }

        [HttpPost]
        public ActionResult AddNewCard(CardsModels cardsModels)
        {
            Thread.Sleep(50);
            GetTypes();
            if (ModelState.IsValid)
            {
                cardsModels.AddedTime = DateTime.Now;
                using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    NpgsqlCommand command2 = new NpgsqlCommand("select count(*) from public.cards", connection);
                    NpgsqlCommand command = new NpgsqlCommand("INSERT INTO public.cards(Name, Type, Image, AddedTime, Cost, information, Id) VALUES (@n, @t, @i, @a, @c, @g, @id)", connection);
                    var reader = command2.ExecuteReader();
                    while (reader.Read())
                    {
                        cardsModels.Id = reader.GetInt64(0);
                        cardsModels.Id++;
                    }
                    command2.Connection.Close();
                    connection.Open();
                    command.Parameters.AddWithValue("n", cardsModels.Name);
                    command.Parameters.AddWithValue("t", cardsModels.Type);
                    command.Parameters.AddWithValue("i", cardsModels.Image);
                    command.Parameters.AddWithValue("a", cardsModels.AddedTime);
                    command.Parameters.AddWithValue("c", cardsModels.Cost);
                    command.Parameters.AddWithValue("g", cardsModels.Information);
                    command.Parameters.AddWithValue("id", cardsModels.Id);
                    command.ExecuteNonQuery();
                    connection.Close();
                    return Redirect("Confirmation");
                }
            }
            return View();
        }

        [Route("RemoveCity/{city}")]
        public ActionResult RemoveCity(string city)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                NpgsqlCommand command=new NpgsqlCommand($"delete from citylist where city='{city}'",connection);
                command.ExecuteNonQuery();
            }

            return RedirectToAction("AddNewCity");
        }

        public ActionResult Confirmation()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Presentation()
        {
            OutputPresentationSort();
            return View();
        }

        [HttpPost]
        public ActionResult Presentation(ManagmentSortModels model)
        {
            string currentModel = "";
            if (model.Id != null) currentModel = model.Id;
            if (model.Type != null) currentModel = model.Type;
            if (model.Remove != null) currentModel = model.Remove;
            if (model.Name != null) currentModel = model.Name;
            if (currentModel == "Remove")
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    NpgsqlCommand command = new NpgsqlCommand("DELETE FROM public.cards WHERE ID=(SELECT MAX(id) FROM cards)", connection);
                    command.ExecuteNonQuery();
                    connection.Close();
                }

                OutputPresentationSort();
                return View();
            } //удаление последней строки

            if (currentModel == "ID")
            {
                if (model.isDescending)
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
                    {
                        connection.Open();
                        NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM public.cards ORDER BY id desc", connection);
                        OutputPresentationSortedBy(command, connection);
                    }
                }
                else
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
                    {
                        connection.Open();
                        NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM public.cards ORDER BY id asc", connection);
                        OutputPresentationSortedBy(command, connection);
                    } //сортировка по ID
                }
            }

            if (currentModel == "Type")
            {
                if (model.isDescending)
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
                    {
                        connection.Open();
                        NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM public.cards ORDER BY type desc", connection);
                        OutputPresentationSortedBy(command, connection);
                    }
                }
                else
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
                    {
                        connection.Open();
                        NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM public.cards ORDER BY type asc", connection);
                        OutputPresentationSortedBy(command, connection);
                    } //сортировка по Type
                }
            }

            if (currentModel == "Name")
            {
                if (model.isDescending)
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
                    {
                        connection.Open();
                        NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM public.cards ORDER BY name desc", connection);
                        OutputPresentationSortedBy(command, connection);
                    }
                }
                else
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
                    {
                        connection.Open();
                        NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM public.cards ORDER BY name asc", connection);
                        OutputPresentationSortedBy(command, connection);
                    } //сортировка по названию
                }
            }
            return View();
        }

        public ActionResult PurchaseManagment()
        {
            List<PurchaseManagmentModels> Data = new List<PurchaseManagmentModels>();
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand("SELECT id, condition FROM public.currentpurchase", connection);
                NpgsqlDataReader dataReader = command.ExecuteReader();
                for (int i = 0; dataReader.Read(); i++)
                {
                    Data.Add(new PurchaseManagmentModels { Id = dataReader.GetInt32(0), Condition = dataReader.GetString(1) });
                }
                connection.Close();
            }
            Data = Data.OrderBy(x => x.Id).ToList();
            ViewBag.Data = Data;

            return View();
        }

        [HttpPost]
        public ActionResult PurchaseManagment(PurchaseManagmentModels model)
        {
            int idToChange = model.Id;
            string currentCondition = model.Condition;
            string nextCondition = "";
            string type = GetTypeOfPurchase(model.Id);
            string mail = "";
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                NpgsqlCommand command =
                    new NpgsqlCommand($"select mail from currentpurchase where id = {idToChange}", connection);
                NpgsqlDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    mail = dataReader.GetString(0);
                }
            }

            if (currentCondition == "handles")
            {
                nextCondition = "delivers";
                string information = "Ваш заказ доставляется!";
                SendEmail(information, mail);
            }

            if (currentCondition == "delivers" && type == "Самовывоз")
            {
                nextCondition = "ready to pickup";
                string information = "Ваш заказ доставлен до точки самовывоза и готов к нему!";
                SendEmail(information, mail);
            }

            if (currentCondition == "delivers" && type == "Доставка до квартиры")
            {
                nextCondition = "finished";
                string information = "Спасибо, что воспользовались нашим интернет магазином! Ждём Вас снова!";
                SendEmail(information, mail);
            }


            if (currentCondition == "ready to pickup")
            {
                nextCondition = "finished";
                string information = "Спасибо, что воспользовались нашим интернет магазином! Ждём Вас снова!";
                SendEmail(information, mail);
            }

            if (nextCondition == "finished")
            {
                string purchase = "";

                using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    NpgsqlCommand command = new NpgsqlCommand($"select purchase from currentpurchase where id = {idToChange}", connection);
                    NpgsqlDataReader dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        purchase = dataReader.GetString(0);
                    }

                    var purchasedProductsArr = purchase.Split(',');

                    string purchasedProducts = "";

                    for (int i = 0; i < purchasedProductsArr.Length; i++)
                    {
                        if (i == purchasedProductsArr.Length - 1) //если последний элемент
                        {
                            purchasedProducts += purchasedProductsArr[i].Split(':')[0];
                        }
                        else
                        {
                            purchasedProducts += purchasedProductsArr[i].Split(':')[0] + ",";
                        }
                    }

                    connection.Close();
                    connection.Open();
                    NpgsqlCommand command2 = new NpgsqlCommand($"delete from currentpurchase where id={idToChange}", connection);
                    command2.ExecuteNonQuery();
                    NpgsqlCommand command3 = new NpgsqlCommand($"update finishedpurchases set purchase='{purchase}', " +
                                                               $"finishedtime='{DateTime.Now}', finished={true}, purchasedproducts='{purchasedProducts}' where id={idToChange}", connection);
                    command3.ExecuteNonQuery();
                    connection.Close();
                }
            }
            else
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    NpgsqlCommand command = new NpgsqlCommand($"update currentpurchase set condition='{nextCondition}' where id={idToChange}", connection);
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }

            List<PurchaseManagmentModels> Data = new List<PurchaseManagmentModels>();

            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand("SELECT id, condition FROM public.currentpurchase", connection);
                NpgsqlDataReader dataReader = command.ExecuteReader();
                for (int i = 0; dataReader.Read(); i++)
                {
                    Data.Add(new PurchaseManagmentModels { Id = dataReader.GetInt32(0), Condition = dataReader.GetString(1) });
                }

                Data = Data.OrderBy(x => x.Id).ToList();
                connection.Close();
            }

            ViewBag.Data = Data;

            return View();
        }

        private string GetTypeOfPurchase(int id)
        {
            string type = "";
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand($"SELECT typeofdelivery FROM public.currentpurchase where id={id}", connection);
                NpgsqlDataReader dataReader = command.ExecuteReader();

                for (int i = 0; dataReader.Read(); i++)
                {
                    type = dataReader.GetString(0);
                }
                connection.Close();
            }

            return type;
        }

        private void OutputPresentationSort()
        {
            List<ManagmentSortModels> Data;
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM public.cards", connection);
                NpgsqlDataReader dataReader = command.ExecuteReader();
                Data = new List<ManagmentSortModels>();
                for (int i = 0; dataReader.Read(); i++)
                {
                    Data.Add(new ManagmentSortModels
                    {
                        Id = dataReader.GetInt64(0).ToString(),
                        Name = dataReader.GetString(1),
                        Type = dataReader.GetString(2),
                        AddedTime = dataReader.GetDateTime(3).ToString(),
                        Image = dataReader.GetString(4),
                        Information = dataReader.GetString(5),
                        Cost = dataReader.GetInt32(6).ToString()
                    });
                }
                connection.Close();
            }
            ViewBag.Data = Data;
        }

        private void OutputPresentationSortedBy(NpgsqlCommand command, NpgsqlConnection connection)
        {
            List<ManagmentSortModels> Data;

            NpgsqlDataReader dataReader = command.ExecuteReader();
            Data = new List<ManagmentSortModels>();
            for (int i = 0; dataReader.Read(); i++)
            {
                Data.Add(new ManagmentSortModels
                {
                    Id = dataReader.GetInt64(0).ToString(),
                    Name = dataReader.GetString(1),
                    Type = dataReader.GetString(2),
                    AddedTime = dataReader.GetDateTime(3).ToString(),
                    Image = dataReader.GetString(4),
                    Information = dataReader.GetString(5),
                    Cost = dataReader.GetInt32(6).ToString()
                });
            }

            connection.Close();
            ViewBag.Data = Data;
        }

        private void SendEmail(string information, string mail)
        {
            SmtpClient client = new SmtpClient();
            client.Host = "smtp.gmail.com";
            client.Port = 587;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential("qqqwertyshop@gmail.com", "1234QWER+");
            client.Send("qqqwertyshop@gmail.com", mail, "Заказ успешно оформлен!", information);
        }

        private void GetTypes()
        {
            List<string> data = new List<string>();
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand($"SELECT type FROM public.types", connection);
                NpgsqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    data.Add(dataReader.GetString(0));
                }
                connection.Close();
            }

            ViewBag.Types = data;
        }

        public ActionResult AddNewType()
        {
            ViewBag.Message = "";
            GetTypes();
            return View();
        }

        [HttpPost]
        public ActionResult AddNewType(CardsModels model)
        {
            ViewBag.Message = "Успешно!";
            AddNewType(model.Type);
            GetTypes();
            return View();
        }

        private void AddNewType(string type)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand($"insert into types(type) values('{type}')", connection);
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public ActionResult AddPropertiesToType()
        {
            GetTypes();
            ViewBag.Message = "";
            return View();
        }

        [HttpPost]
        public ActionResult AddPropertiesToType(AddPropertiesToTypeModels model)
        {
            GetTypes();
            AddPropertiesToType(model.Properties, model.Type);
            ViewBag.Message = "Успешно!";
            return View();
        }

        private void AddPropertiesToType(string properties, string type)
        {
            var propertiesArr = properties.Split(',');
            for (int i = 0; i < propertiesArr.Length; i++)
            {
                if (char.IsWhiteSpace(propertiesArr[i][0]))
                    propertiesArr[i] = propertiesArr[i].Remove(0, 1);
                if (char.IsWhiteSpace(propertiesArr[i][propertiesArr[i].Length - 1]))
                    propertiesArr[i] = propertiesArr[i].Remove(propertiesArr[i].Length - 1, 1);
            }

            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand("UPDATE types SET properties = @p WHERE type=@t", connection);
                command.Parameters.AddWithValue("t", type);
                command.Parameters.AddWithValue("p", propertiesArr);
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        private void GetViewOfCity()
        {
            List<string> city=new List<string>();
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand("Select city from public.citylist", connection);

                NpgsqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    city.Add(dataReader.GetString(0));
                }
                connection.Close();
            }

            ViewBag.CityList = city;
        }
    }
}