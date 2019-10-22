using System.ComponentModel.DataAnnotations;

namespace QWERTYShop.Models
{
    public class PaymentModels
    {
        [Display(Name = "Номер карты")]
        [Required(ErrorMessage = "Введите номер карты")]
        public string CardNumber { get; set; }

        [Display(Name = "CVC")]
        [Required(ErrorMessage = "Введите CVC")]
        public string CVC { get; set; }

        [Display(Name = "Месяц/год до которого доступна карта")]
        [Required(ErrorMessage = "Введите месяц и год")]
        public string DataValidation { get; set; }

        [Display(Name = "Название фамилию и имя указанные на карте")]
        [Required(ErrorMessage = "Заполните это поле!")]
        public string Person { get; set; }
    }
}