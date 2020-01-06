using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Npgsql;
using QWERTYFSharp.Helpers;
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
            GetFilters(category);
            var maxPrice = GetMaxPrice(category);
            ViewBag.Values = new float[] { 0, maxPrice, 0, 10 };
            ViewBag.Sorted = "По рейтингу";
            return View();
        }

        [HttpPost]
        [Route("category/{category}")]
        public ActionResult Category(string category, CategoryModels model) //в модели есть лист значений чекбоксов model.Check
        {
            GetFilteringValues(category, model);
            GetFilters(category);
            GetSortedCategories(category, model);
            ViewBag.Max = GetMaxPrice(category);
            ViewBag.Sorted = model.Method;
            return View();
        }

        private void GetFilteringValues(string category, CategoryModels model)
        {
            ViewBag.Values = new float[] { model.FromPrice, model.ToPrice, model.FromRating, model.ToRating };
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
                string cmd = "";
                if (model.Check.Contains(true))
                {
                    cmd = $"select * from cards where type='{category}' and cost>={model.FromPrice} and cost<={model.ToPrice} " +
                         $" and averagemark>={model.FromRating} and averagemark<={model.ToRating} {GetInsertString(category, model)} order by {orderType} {typeOfSort}";
                }
                else
                {
                    cmd =
                        $"select * from cards where type='{category}' and cost>={model.FromPrice} and cost<={model.ToPrice} " +
                        $" and averagemark>={model.FromRating} and averagemark<={model.ToRating} order by {orderType} {typeOfSort}";
                }

                var command = new NpgsqlCommand(cmd, connection);

                connection.Open();
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
                        value = reader.GetInt32(0);
                    }
                }
                connection.Close();
            }

            return value;
        }

        private void GetFilters(string category)
        {
            var dictionary = new Dictionary<string, Dictionary<int, string>>();
            List<string> properties = new List<string>();
            string specialName = "";
            using (var connection = new NpgsqlConnection(ConnectionString)) //получение свойств и specialname
            {
                connection.Open();
                var cmd = new NpgsqlCommand($"select choosefilter, specialname from types where type='{category}'", connection);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    properties = ((string[])reader.GetValue(0)).ToList();
                    specialName = reader.GetString(1);
                }
                connection.Close();
            }

            int i = 0;

            foreach (var property in properties)
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    var subDict = new Dictionary<int, string>();
                    connection.Open();
                    var cmd = new NpgsqlCommand($"select {property.Replace(' ', '_')} from {specialName}", connection);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        if (!subDict.ContainsValue(reader.GetString(0)))
                        {
                            subDict.Add(i, reader.GetString(0));
                            i++;
                        }
                    }
                    connection.Close();
                    dictionary.Add(property, subDict);
                }
            }
            ViewBag.Dictionary = dictionary;
        }

        private string GetInsertString(string category, CategoryModels model)
        {
            var accordances = new Accordances(model.Check, ViewBag.Dictionary, category);
            string result = "and(";

            Dictionary<string, List<long>> structure=new Dictionary<string, List<long>>();

            for (int i = 0; i < accordances.Check.Count; i++)
            {
                if (accordances.Check[i])
                {
                    var ids = GetCategoryIds(accordances, i);
                    var property = accordances.GetProperty(i);
                    if (!structure.ContainsKey(property))
                    {
                        structure.Add(property, ids);
                    }
                    else
                    {
                        var currentList = structure[property];
                        structure[property]=currentList.Union(ids).ToList();
                    }
                }
            }

            foreach (var element in structure)
            {
                for (int i = 0; i < element.Value.Count; i++)
                {
                    if (i == 0)
                        result += $"(id={element.Value[i]} or ";
                    else
                        result += $"id={element.Value[i]} or ";

                }

                result = result.Remove(result.Length - 4, 4);
                result += ") and ";
            }

            result = result.Remove(result.Length - 5, 5);
            result += ")";
            return result;
        }

        public List<long> GetCategoryIds(QWERTYFSharp.Helpers.Accordances accs, int i) //получает лист id(в cards) по выборке
        {
            List<long> list = new List<long>();
            string specialName = new ManagmentController().GetSpecialNameUsingType(accs.Category);

            using (NpgsqlConnection connection = new NpgsqlConnection(Connection.ConnectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand($"select id from {specialName} where \"{accs.GetProperty(i)}\" = '{accs.GetValue(i)}' ", connection);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(reader.GetInt64(0));
                }
            }

            return list;
        }
    }
}