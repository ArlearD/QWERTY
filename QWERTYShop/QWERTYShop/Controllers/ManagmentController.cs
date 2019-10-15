using Npgsql;
using QWERTYShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace QWERTYShop.Controllers
{
    [Authorize(Roles="admin")]
    public class ManagmentController:Controller
    {
        string ConnectionString = "Server = localhost; Port=5432; Database=postgres; User Id =postgres; Password=1234QWER+";
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(CardsModels cardsModels)
        {
            Thread.Sleep(50);
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
                    return Redirect("Managment/Confirmation");
                }
            }
            return View();
        }

        public ActionResult Confirmation()
        {
            return View();
        }

        public ActionResult Presentation()
        {
            List<string> Data;
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM public.cards", connection);
                NpgsqlDataReader dataReader = command.ExecuteReader();
                Data = new List<string>();
                for (int i = 0; dataReader.Read(); i++)
                {
                    Data.Add("ID: "+dataReader[0].ToString() + ", name: " + dataReader[1].ToString() + ", type: " + dataReader[2].ToString() + ", added time: " +
                        dataReader[3].ToString() + ", image: " + dataReader[4].ToString() + ", information: " + dataReader[5].ToString() 
                        +" cost: "+ dataReader[6].ToString()+"\r\n");
                }
                connection.Close();
            }
            ViewBag.Data = Data;
            Data = null;
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
            bool IsDescending = false;
            string previousCommand = currentModel;
            if (currentModel == "Remove")
            {
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand("DELETE FROM public.cards WHERE ID=(SELECT MAX(id) FROM cards)", connection);
                command.ExecuteNonQuery();
                connection.Close();
            }

            List<string> Data;
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM public.cards", connection);
                NpgsqlDataReader dataReader = command.ExecuteReader();
                Data = new List<string>();
                for (int i = 0; dataReader.Read(); i++)
                {
                    Data.Add("ID: " + dataReader[0].ToString() + ", name: " + dataReader[1].ToString() + ", type: " + dataReader[2].ToString() + ", added time: " +
                        dataReader[3].ToString() + ", image: " + dataReader[4].ToString() + ", information: " + dataReader[5].ToString()
                        + " cost: " + dataReader[6].ToString() + "\r\n");
                }
                connection.Close();
            }
            ViewBag.Data = Data;
            Data = null;
            return View();
            } //удаление последней строки

            if (currentModel == "ID")
            {
                List<string> Data = new List<string>();
                using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM public.cards", connection);
                    NpgsqlDataReader dataReader = command.ExecuteReader();
                    for (int i = 0; dataReader.Read(); i++)
                    {
                        Data.Add("ID: " + dataReader[0].ToString() + ", name: " + dataReader[1].ToString() + ", type: " + dataReader[2].ToString() + ", added time: " +
                            dataReader[3].ToString() + ", image: " + dataReader[4].ToString() + ", information: " + dataReader[5].ToString()
                            + " cost: " + dataReader[6].ToString() + "\r\n");
                    }
                    if (previousCommand == "id"&&IsDescending==true)
                    {
                        Data=Data
                        .OrderByDescending(x => x[0])
                        .ToList();
                        IsDescending = false;
                    }
                    else
                    {
                        Data=Data
                       .OrderBy(x => x[0])
                       .ToList();
                        IsDescending = true;
                    }
                    connection.Close();
                }
                ViewBag.Data = Data;
            }



            return View();
        }
    }
}