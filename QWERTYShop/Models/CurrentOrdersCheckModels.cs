﻿using System.ComponentModel.DataAnnotations;

namespace QWERTYShop.Models
{
    public class CurrentOrdersCheckModels
    {
        [Required(ErrorMessage = "Заполните это поле!")]
        [Display(Name="Введите электронную почту, которую использовали при оформлении заказа")]
        public string Mail { get; set; }

        [Required(ErrorMessage = "Заполните это поле!")]
        [Display(Name="Введите номер заказа, его можно посмотреть на Вашей электронной почте")]
        public long Id { get; set; }

        public string Purchase { get; set; }

        public string Delivery { get; set; }

        public string Condition { get; set; }

        public bool Paid { get; set; }

        public override string ToString()
        {
            return $"Mail:={Mail} Id:={Id}";
        }
    }
}