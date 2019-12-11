using System;
using System.Collections.Generic;
using Npgsql;
using QWERTYShop.Controllers;

namespace QWERTYShop.Helpers
{
    public class Accordances
    {
        public List<bool> Check { get; set; }
        public Dictionary<string, Dictionary<int, string>> Dictionary { get; set; }
        public string Type { get; set; }

        public Accordances(List<bool> check, Dictionary<string, Dictionary<int, string>> dictionary, string category)
        {
            Check = check;
            Dictionary = dictionary;
            Type = category;
        }

        public string GetProperty(int i) //получает свойство текущего id
        {
            foreach (var property in Dictionary)
            {
                foreach (var value in property.Value)
                {
                    if (value.Key == i) return property.Key.Replace(' ', '_');
                }
            }
            throw new Exception("Неверный i");
        }

        public string GetValue(int i) //получает значение строки по id
        {
            foreach (var property in Dictionary)
            {
                foreach (var value in property.Value)
                {
                    if (value.Key == i) return value.Value;
                }
            }
            throw new Exception("Неверный i");
        }

        public List<long> GetIds(int i) //получает лист id(в cards) по выборке
        {
            List<long> list = new List<long>();
            string specialName = new ManagmentController().GetSpecialNameUsingType(Type);

            using (NpgsqlConnection connection = new NpgsqlConnection(Connection.ConnectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand($"select id from {specialName} where \"{GetProperty(i)}\" = '{GetValue(i)}' ", connection);
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