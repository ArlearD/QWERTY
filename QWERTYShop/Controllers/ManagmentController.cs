using Npgsql;
using QWERTYShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security.OAuth;

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
                    NpgsqlCommand command = new NpgsqlCommand($"INSERT INTO public.citylist(City, IsAvailableForPickup, CostForDelivery, addresses) VALUES (@c, @i, @cfd, '{{}}')", connection);
                    command.Parameters.AddWithValue("c", model.City);
                    command.Parameters.AddWithValue("i", model.IsAvailableForPickup);
                    command.Parameters.AddWithValue("cfd", model.CostForDelivery);
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                ViewBag.AddNewCitySuccess = "Успешно!";
            }
            else
            {
                ViewBag.AddNewCitySuccess = "Что-то пошло не так, попробуйте снова!";
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
            Thread.Sleep(5);
            GetTypes();
            if (ModelState.IsValid)
            {
                Session["cmd"] = GetCommandForInformationCard(cardsModels);
                return Redirect($"/AddInformation/{cardsModels.Type}");
            }
            return View();
        }

        private void UploadImage(HttpPostedFileBase upload, long id)
        {
            string extension = System.IO.Path.GetFileName(upload.FileName).Split('.').Last();
            string fileName = "";
            fileName += id + "."+ extension;
            upload.SaveAs(Server.MapPath("~/Images/" + fileName));
        }


        [HttpGet]
        [Route("AddInformation/{type}")]
        public ActionResult AddInformation(string type)
        {
            GetProperties(type);
            return View();
        }

        [HttpPost]
        [Route("AddInformation/{type}")]
        public ActionResult AddInformation(List<string> values)
        {
            string type = Session["Type"].ToString();
            var specialName = GetSpecialNameUsingType(type);
            var command2 = GetCommandToAddInSpecialNameTable(values, specialName);
            AddNewCard(Session["cmd"].ToString(), command2);
            return RedirectToAction("Confirmation");
        }

        private void GetProperties(string type)
        {
            string[] properties = null;
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand($"Select properties from types where type='{type}'", connection);

                NpgsqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    properties = ((string[])dataReader.GetValue(0)).OrderBy(x => x).ToArray();
                }
                connection.Close();
            }
            ViewBag.Properties = properties;
        }

        private List<string> GetListOfProperties(string type)
        {
            List<string> properties = null;
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand($"Select properties from types where type='{type}'", connection);

                NpgsqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    properties = ((string[])dataReader.GetValue(0)).OrderBy(x => x).ToList();
                }
                connection.Close();
            }
            return properties;
        }

        [Route("RemoveCity/{city}")]
        public ActionResult RemoveCity(string city)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand($"delete from citylist where city='{city}'", connection);
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
            if (currentModel == "Remove last")
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

        private void GetTypes() //нужно переделать под specialname-
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
            AddNewType(model.Type, model.SpecialName);
            GetTypes();
            return View();
        }

        private void AddNewType(string type, string specialName)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand($"insert into types(type, specialname, choosefilter) values('{type}', '{specialName}', '{{}}')", connection);
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
            CreateNewTable(propertiesArr, type);
        }

        private void GetViewOfCity()
        {
            List<string> city = new List<string>();
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

        private void CreateNewTable(string[] propertiesArr, string type)
        {
            string body = "";
            string specialName = GetSpecialNameUsingType(type);
            specialName = specialName.Replace(' ', '_');
            string current = $"create table if not exists {specialName}(id bigint, ";

            for (int i = 0; i < propertiesArr.Length; i++)
            {
                for (int j = 0; j < propertiesArr[i].Length; j++)
                {
                    if (!Char.IsLetterOrDigit(propertiesArr[i], j))
                    {
                        propertiesArr[i] = propertiesArr[i].Replace(propertiesArr[i][j], '_');
                    }
                }
                if (propertiesArr.Length - 1 == i)
                    body += propertiesArr[i] + " varchar);";
                else
                {
                    body += propertiesArr[i] + " varchar, ";
                }
            }

            current += body;
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand(current, connection);
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public string GetSpecialNameUsingType(string type)
        {
            string result = "";
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand($"Select specialname from types where type='{type}'", connection);
                NpgsqlDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    result = dataReader.GetString(0);
                }
                connection.Close();
            }
            return result.Replace(' ', '_').ToLower();
        }

        private string GetCommandForInformationCard(CardsModels cardsModels)
        {
            cardsModels.AddedTime = DateTime.Now;
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand("select count(*) from public.cards", connection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    cardsModels.Id = reader.GetInt64(0);
                    cardsModels.Id++;
                }
                connection.Close();
            }

            HttpPostedFileBase upload = cardsModels.Upload;
            if (upload != null)
            {
                UploadImage(upload, cardsModels.Id);
            }

            Session["Type"] = cardsModels.Type;

            var cmd =
                $"INSERT INTO cards(Name, Type, Image, AddedTime, Cost, information, averagemark, Id) VALUES ('{cardsModels.Name}', '{cardsModels.Type}', '{cardsModels.Image}'" +
                $", '{cardsModels.AddedTime}', {cardsModels.Cost}, '{cardsModels.Information}', 10, {cardsModels.Id})";
            return cmd;
        }

        private void AddNewCard(string command, string command2)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString)) //в таблицу cards
            {
                connection.Open();
                NpgsqlCommand cmd = new NpgsqlCommand(command, connection);
                cmd.ExecuteNonQuery();
                NpgsqlCommand cmd2 = new NpgsqlCommand(command2, connection);
                cmd2.ExecuteNonQuery();
                connection.Close();
            }
        }

        private string GetCommandToAddInSpecialNameTable(List<string> anotherProperties, string specialName)
        {
            StringBuilder idBuilder = new StringBuilder();
            var badId = Session["cmd"].ToString().Split(',').Last();
            for (int i = 0; i < badId.Length; i++)
                if (Char.IsDigit(badId[i])) idBuilder.Append(badId[i]);
            long id = long.Parse(idBuilder.ToString());
            StringBuilder builder = new StringBuilder($"INSERT INTO {specialName} VALUES({id}, ");
            for (int i = 0; i < anotherProperties.Count; i++)
            {
                if (anotherProperties.Count - 1 == i)
                    builder.Append($"'{anotherProperties[i]}')");
                else
                    builder.Append($"'{anotherProperties[i]}', ");
            }
            return builder.ToString();
        }

        public ActionResult Filtering()
        {
            GetCategories();
            ViewBag.Filters = new List<string>();
            ViewBag.Properties = new List<string>();
            ViewBag.Category = "";
            return View();
        }

        [HttpPost]
        public ActionResult Filtering(List<string> Category, FilteringModels model)
        {
            DoCommand(model.Operation, model.Property, Category[0]);
            GetCategories();
            var properties = GetListOfProperties(Category[0]);
            var filters = GetFilters(Category[0]);
            filters = properties.Intersect(filters).ToList();
            properties = properties.Except(filters).ToList();
            ViewBag.Properties = properties;
            ViewBag.Filters = filters;
            ViewBag.Category = Category[0];
            if (model.Operation == null)
            {
                return View();
            }
            return View(model);
        }

        private List<string> GetFilters(string category)
        {
            List<string> properties = new List<string>();
            using (var connection = new NpgsqlConnection(ConnectionString)) //получение свойств
            {
                connection.Open();
                var cmd = new NpgsqlCommand($"select choosefilter from types where type='{category}'", connection);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    properties = ((string[])reader.GetValue(0)).OrderBy(x => x).ToList();
                }
                connection.Close();
            }

            return properties;
        }

        private void GetCategories()
        {
            List<string> categories = new List<string>();
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                NpgsqlCommand cmd = new NpgsqlCommand("select type from types", connection);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    categories.Add(reader.GetString(0));
                }
            }
            ViewBag.Categories = categories;
        }

        private void DoCommand(string command, string propertyName, string type)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                if (command == "Добавить фильтр")
                {
                    NpgsqlCommand cmd = new NpgsqlCommand($"update types set choosefilter = choosefilter || " +
                                                          $"'{{\"{propertyName}\"}}' where type='{type}'", connection); //вставка в массив
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    NpgsqlCommand cmd = new NpgsqlCommand($"update types SET choosefilter = " +
                                                          $"array_remove(choosefilter, '{propertyName}')", connection); //удаление из массива
                    cmd.ExecuteNonQuery();
                }
                connection.Close();
            }

            if (command == "Добавить фильтр")
                ViewBag.Message = $"Категории {type} добавлена фильтрация по свойству: {propertyName}";
            else
                ViewBag.Message = $"Успешно удалена фильтрация по свойству: {propertyName} из категории {type}";

        }

        public ActionResult PickupLocation()
        {
            GetViewOfCity();
            ViewBag.Message = "";
            return View();
        }

        [HttpPost]
        public ActionResult PickupLocation(List<string> list)
        {
            GetViewOfCity();
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand(
                    $"update citylist set addresses=addresses || '{{\"{list[1]}\"}}' where city='{list[0]}'", connection);
                cmd.ExecuteNonQuery();
                connection.Close();
            }

            ViewBag.Message = "Успешно!";
            return View();
        }
    }
}