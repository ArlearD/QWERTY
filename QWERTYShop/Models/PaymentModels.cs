using System.ComponentModel.DataAnnotations;

namespace QWERTYShop.Models
{
    public class PaymentModels
    {
        [Display(Name = "Номер карты")]
        [Required(ErrorMessage = "Введите номер карты")]
        [RegularExpression("[0-9]{16}", ErrorMessage = "Введите номер карты корректно!")]
        public string CardNumber { get; set; }

        [RegularExpression("[0-9]{3}", ErrorMessage = "Введите CVC корректно!")]
        [Display(Name = "CVC")]
        [Required(ErrorMessage = "Введите CVC")]
        public string CVC { get; set; }

        [Display(Name = "Месяц до которого доступна карта")]
        public string MonthValidation { get; set; }

        [Display(Name = "Год до которого доступна карта")]
        public string YearValidation { get; set; }

        [RegularExpression(@"[A-Za-z\s]+$", ErrorMessage = "Введите фамилию и имя корректно!")]
        [Display(Name = "Укажите фамилию и имя указанные на карте")]
        [Required(ErrorMessage = "Заполните это поле!")]
        public string Person { get; set; }
    }
}