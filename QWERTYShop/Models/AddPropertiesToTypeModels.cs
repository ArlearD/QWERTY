using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QWERTYShop.Models
{
    public class AddPropertiesToTypeModels
    {
        [Required]
        [Display(Name = "Выберите тип")]
        public string Type { get; set; }

        [Display(Name = "Напишите через запятую важные характеристики для данного типа (появится возможность группировать)")]
        public string Properties { get; set; }
    }
}