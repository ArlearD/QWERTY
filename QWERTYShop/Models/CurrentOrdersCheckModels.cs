using System.ComponentModel.DataAnnotations;

namespace QWERTYShop.Models
{
    public class CurrentOrdersCheckModels
    {
        [Required(ErrorMessage = "заполните это поле!")]
        [Display(Name="Введите электронную почту, которую использовали при оформлении заказа")]
        public string Mail { get; set; }

        [Required(ErrorMessage = "заполните это поле!")]
        [Display(Name="Введите номер заказа, его можно посмотреть на Вашей электронной почте")]
        public long Id { get; set; }
    }
}