using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QWERTYShop.Models
{
    public class ManagmentSortModels
    {
        public string Remove { get; set; }
        public string Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Information { get; set; }
        public string Image { get; set; }
        public string AddedTime { get; set; }
        public string Cost { get; set; }
        public bool isDescending { get; set; }
    }
}