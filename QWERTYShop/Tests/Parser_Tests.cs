using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NUnit.Framework;

namespace QWERTYShop.Tests
{
    [TestFixture]
    public class Parser_Tests
    {
        private readonly List<Models.CartModels> singleCartElement = new List<Models.CartModels>()
        { new Models.CartModels()
            {
            Cost = 10,
            Count = 2,
            Id = 1,
            Method = "+",
            Name = "Product 1"
            }
        };

        private readonly List<Models.CartModels> manyCartElements = new List<Models.CartModels>()
        {
            new Models.CartModels()
            {
                Cost = 10,
                Count = 2,
                Id = 1,
                Method = "+",
                Name = "Product 1"
            },
            new Models.CartModels()
            {
                Cost = 100,
                Count = 3,
                Id = 2,
                Method = "-",
                Name = "Product 2"
            },
            new Models.CartModels()
            {
                Cost = 30,
                Count = 1,
                Id = 3,
                Method = "+",
                Name = "Product 3"
            }
        };

        [Test]
        public void ParseSingleCartElement()
        {
            var str = Models.Parser.ParseCartElements(singleCartElement);
            Assert.AreEqual("1:2", str);
        }

        [Test]
        public void ParseManyCartElements()
        {
            var str = Models.Parser.ParseCartElements(manyCartElements);
            Assert.AreEqual("1:2,2:3,3:1", str);
        }
    }
}