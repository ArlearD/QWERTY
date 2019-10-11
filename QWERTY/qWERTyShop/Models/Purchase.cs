using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace qWERTyShop.Models
{
    public class Purchase
    {
        public int PurchaseID { get; set; }
        public int CardID { get; set; }
        public string Login { get; set; }
        public string Address { get; set; }
        public int Price { get; set; }
    }
}
