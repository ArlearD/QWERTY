using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace qWERTyShop.Models
{
    public class Registration
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string RePassword { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string Flag { get; set; }
    }
}
