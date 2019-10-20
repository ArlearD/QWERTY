using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QWERTYShop.Models
{
    public class AddNewCityModels
    {
        [Display(Name = "Название города")]
        [Required(ErrorMessage = "Требуется название города!")]
        public string City { get; set; }

        [Display(Name = "Есть ли в городе пункты самовывоза?")]
        public bool IsAvailableForPickup { get; set; }

        [Display(Name = "Цена доставки до квартиры")]
        [Required(ErrorMessage = "Требуется цена доставки!")]
        public int CostForDelivery { get; set; }
    }
}