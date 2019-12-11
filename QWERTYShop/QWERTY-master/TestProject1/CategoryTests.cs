using System;
using System.Collections.Generic;
using NUnit.Framework;
using QWERTYShop;
using QWERTYShop.Controllers;
using Npgsql;

namespace TestProject1
{
    [TestFixture]
    public class CategoryTests
    {
        [Test]
        public void RightNumberOfCategories()
        {
            var actual=new ManagmentController().GetCategories().Count;

            long expected = 0;
            
            using (NpgsqlConnection connection = new NpgsqlConnection(Connection.ConnectionString))
            {
                connection.Open();
                NpgsqlCommand cmd = new NpgsqlCommand("select count(type) from types", connection);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    expected = reader.GetInt64(0);
                }
            }
            
            Assert.AreEqual(expected, actual);
        }
    }
}