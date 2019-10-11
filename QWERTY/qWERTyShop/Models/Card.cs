using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace qWERTyShop.Models
{
    public class Card
    {
        public int ID { get; set; }
        public enum Type
        {
            TV,
            PC,
            Phones
        }

    }
}
