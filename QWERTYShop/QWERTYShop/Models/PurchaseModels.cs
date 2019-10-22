using System.ComponentModel.DataAnnotations;

namespace QWERTYShop.Models
{
    public class PurchaseModels
    {
        public string Method { get; set; }
        public string IsAvailableForPickup { get; set; }

        [Required(ErrorMessage = "!Введите название города!")]
        [Display(Name = "Название города")]
        public string City { get; set; }

        [Required(ErrorMessage = "!Введите название улицы!")]
        [Display(Name = "Улица")]
        public string Street { get; set; }

        [Required(ErrorMessage = "!Введите номер дома!")]
        [Display(Name = "Дом")]
        [RegularExpression(@"^[0,9]", ErrorMessage = "Некорректный номер дома")]
        public int House { get; set; }

        [Required(ErrorMessage = "!Введите номер квартиры!")]
        [Display(Name = "Номер квартиры")]
        public int Flat { get; set; }

        [Required(ErrorMessage = "!Введите номер телефона!")] 
        [Display(Name = "Номер телефона с восьмёркой")]
        [RegularExpression(@"^([0-9]{10}|[0-9]{12})$", ErrorMessage = "Некорректный номер телефона")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "!Введите почту!")]
        [Display(Name = "Электронная почта для уведомлений о смене статуса заказа")]
        [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}", ErrorMessage = "Некорректный адрес")]
        public string Mail { get; set; }

        public string Payment { get; set; }

    }
}