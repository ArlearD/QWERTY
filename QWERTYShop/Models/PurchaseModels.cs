using System.ComponentModel.DataAnnotations;

namespace QWERTYShop.Models
{
    public class PurchaseModels
    {
        public string Method { get; set; }

        [Display(Name = "Название города")]
        public string City { get; set; }

        [Required(ErrorMessage = "Заполните это поле!")]
        [RegularExpression(@"^[А-Яа-яЁё\s]+$", ErrorMessage = "Некорректное название улицы")]
        [Display(Name = "Улица")]
        public string Street { get; set; }

        [Required(ErrorMessage = "Заполните это поле!")]
        [Display(Name = "Дом")]
        [RegularExpression(@"[0-9А-Яа-яЁё/-]{1,8}", ErrorMessage = "Некорректный номер дома")]
        public string House { get; set; }

        [Required(ErrorMessage = "Заполните это поле!")]
        [Display(Name = "Номер квартиры")]
        [RegularExpression("[0-9]{1,5}", ErrorMessage = "Некорректный номер квартиры")]
        public int Flat { get; set; }

        [Required(ErrorMessage = "Заполните это поле!")] 
        [Display(Name = "Номер телефона с восьмёркой")]
        [RegularExpression(@"[8]{1}[0-9]{10}", ErrorMessage = "Некорректный номер телефона")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Заполните это поле!")]
        [Display(Name = "Электронная почта для уведомлений о смене статуса заказа")]
        [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}", ErrorMessage = "Некорректный адрес")]
        public string Mail { get; set; }

        [Display(Name = "Желаемая дата доставки")]
        public string DeliveryDate { get; set; }

        [Display(Name = "Желаемое время доставки")]
        public string DeliveryTime { get; set; }

        [Display(Name = "Желаемая дата получения доставки")]
        public string PickupDate { get; set; }

        public string Payment { get; set; }

    }
}