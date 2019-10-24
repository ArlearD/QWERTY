using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QWERTYShop.Models
{
    public class CartModels
    {
        public long Id { get; set; }
        public int Count { get; set; }
        public string Name { get; set; }
        public int Cost { get; set; }
        public string Method { get; set; }

        public override string ToString()
        {
            return $"Id:={Id} Count:={Count} Name:={Name} Price:={Cost}";
        }
    }
}