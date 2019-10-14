using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QWERTYShop.Models
{
    public class CardsModels
    {
        public long Id { get; set; }

        public DateTime AddedTime { get; set; }

        [Required(ErrorMessage = "Введите название товара")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Введите цену")]
        public int Cost { get; set; }

        public string Image { get; set; }

        public string Information { get; set; }

        [Required(ErrorMessage = "Введите тип продукта")]
        public string Type { get; set; }
    }
}