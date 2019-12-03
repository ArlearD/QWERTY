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

        [Display(Name = "Название товара")]
        [Required(ErrorMessage = "Введите название товара")]
        public string Name { get; set; }

        [Display(Name = "Стоимость товара")]
        [Required(ErrorMessage = "Введите цену")]
        public int Cost { get; set; }

        [Display(Name = "Ссылка на изображение товара")]
        public string Image { get; set; }

        [Display(Name = "Дополнительная информация о товаре")]
        public string Information { get; set; }

        [Display(Name = "Категория товара")]
        [Required(ErrorMessage = "Введите тип продукта")]
        public string Type { get; set; }

        [Display(Name="Также можете просто загрузить картинку на сервер(.jpg .jpeg .png). " +
                      "Обратите внимание на то, что если вы загрузите и то и другое, то выведется вариант с ссылкой")]
        public HttpPostedFileBase Upload { get; set; }

        public override string ToString()
        {
            return $"Id:={Id} Name:={Name} Cost:={Cost} Image:={Image} Information:={Information} Type:={Type}";
        }
        [RegularExpression("[A-Za-z]", ErrorMessage = "Только английские буквы!")]
        public string SpecialName { get; set; }
    }
}