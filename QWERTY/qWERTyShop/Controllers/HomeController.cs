using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using qWERTyShop.Models;
using Npgsql;


namespace qWERTyShop.Controllers
{
    public class HomeController : Controller
    {
        string connectionString = "Server = localhost; Port=5432; Database=postgres; User Id =postgres; Password=1234QWER+";
        public IActionResult Index()
        {
            List<string> Data;
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM public.users", connection);
                NpgsqlDataReader dataReader = command.ExecuteReader();
                Data = new List<string>();
                for (int i = 0; dataReader.Read(); i++)
                {
                    Data.Add(dataReader[1].ToString() + "," + dataReader[2].ToString() + "," + dataReader[3].ToString()+ "," + dataReader[4].ToString()+"\r\n");
                }
                connection.Close();
            }
            ViewBag.Data = Data;
            Data = null;
            return View();
        }

        public IActionResult AuthorizationPage()
        {
            return View();
        }

        [HttpPost]
        public string AuthorizationPage(Authorization authorization)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand("SELECT Login, Password FROM public.users;", connection))
                {
                    var reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var exceptedLogin = reader.GetString(0);
                            var exceptedPassword = reader.GetString(1);

                            if (authorization.Login == exceptedLogin && authorization.Password == exceptedPassword)
                            {
                                connection.Close();
                                return "Удачно!";
                            }
                        }
                    }
                }
                connection.Close();
                return "Неудачно!";
            } 
        }

        [HttpGet]
        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registration([Bind("Login", "Password", "RePassword")]Registration registration)
        {
            ViewBag.Message = "";
            if (ModelState.IsValid)
            {
                registration.RegistrationDate = DateTime.Now;
                registration.Flag = "common";
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    NpgsqlCommand command = new NpgsqlCommand("INSERT INTO public.users(Login, Password, Flag, RegistrationTime) VALUES (@l, @p, @f, @r)", connection);
                    command.Parameters.AddWithValue("l", registration.Login);
                    command.Parameters.AddWithValue("p", registration.Password);
                    command.Parameters.AddWithValue("f", registration.Flag);
                    command.Parameters.AddWithValue("r", registration.RegistrationDate);
                    command.ExecuteNonQuery();
                    connection.Close();
                    return RedirectToAction("RegistrationConfirm");
                }
            }
            return View(registration);
        }

        public IActionResult RegistrationConfirm()
        {
            return View();
        }

        public IActionResult DeleteData()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand("DELETE FROM public.users WHERE ID=(SELECT MAX(id) FROM users)", connection);
                command.ExecuteNonQuery();
                connection.Close();
            }
            return View();
        }
    }
}
