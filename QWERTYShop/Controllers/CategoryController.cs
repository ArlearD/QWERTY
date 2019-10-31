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
            return View();
        }
    }
}