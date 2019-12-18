using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QWERTYShop.Models
{
    public class AddPropertiesToTypeModels
    {
        [Required]
        [Display(Name = "Выберите тип")]
        public string Type { get; set; }

        [Display(Name = "Напишите через запятую характеристики для данного типа")]
        public string Properties { get; set; }
    }
}