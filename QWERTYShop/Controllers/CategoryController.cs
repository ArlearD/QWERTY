using System.Collections.Generic;
using System.Web.Mvc;
using Npgsql;
using QWERTYShop.Models;

namespace QWERTYShop.Controllers
{
    public class CategoryController : Controller
    {
        private readonly string ConnectionString =
            Connection.ConnectionString;

        [Route("category/{category}")]
        public ActionResult Category(string category)
        {
            GetCategories(category);
            var maxPrice = GetMaxPrice(category);
            ViewBag.Values = new float[] { 0, maxPrice, 0, 10 };
            ViewBag.Sorted = "По рейтингу";
            return View();
        }

        [HttpPost]
        [Route("category/{category}")]
        public ActionResult Category(string category, CategoryModels model) //method
        {
            GetFilteringValues(category, model);
            GetSortedCategories(category, model);
            ViewBag.Max = GetMaxPrice(category);
            ViewBag.Sorted = model.Method;
            return View();
        }

        private void GetFilteringValues(string category, CategoryModels model)
        {
            ViewBag.Values = new float[] {model.FromPrice, model.ToPrice, model.FromRating, model.ToRating};
        }

        private void GetCategories(string category)
        {
            List<CardsModels> cards = new List<CardsModels>();
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand($"select * from cards where type='{category}';", connection))
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
                            cards.Add(card);
                        }
                }

                ViewBag.Type = category;
                ViewBag.cards = cards;
            }
        }

        private void GetSortedCategories(string category, CategoryModels model)
        {
            string orderType;
            if (model.Method == "По цене")
                orderType = "cost";
            else
                orderType = "averagemark";

            string typeOfSort;
            if (model.IsDescending)
                typeOfSort = "desc";
            else
                typeOfSort = "asc";

            List<CardsModels> cards = new List<CardsModels>();
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command =
                    new NpgsqlCommand($"select * from cards where type='{category}' and cost>={model.FromPrice} and cost<={model.ToPrice} " +
                                      $" and averagemark>={model.FromRating} and averagemark<={model.ToRating} order by {orderType} {typeOfSort}",
                        connection))
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
                            cards.Add(card);
                        }
                }

                ViewBag.Type = category;
                ViewBag.cards = cards;
            }
        }

        private float GetMaxPrice(string category)
        {
            float value = 0;
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand($"select max(cost) from cards where type='{category}' and cost is not null", connection);
                var reader = cmd.ExecuteReader();

                while (reader.Read()) 
                {
                    if (!reader.IsDBNull(0)) 
                    {
                        value = reader.GetInt32(0); //сделать "возврат" значений 
                    }
                }
                connection.Close();
            }

            return value;
        }
    }
}