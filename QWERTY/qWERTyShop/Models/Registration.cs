using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace qWERTyShop.Models
{
    public class Registration
    {
        [Required(ErrorMessage ="!Введите логин!")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "!Длина логина должна быть от 3 до 20 символов")]
        [Display(Name = "Логин")]
        public string Login { get; set; }


        [Required(ErrorMessage = "!Введите пароль!")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "!Длина пароля должна быть от 6 до 20 символов")]
        [Display(Name = "Пароль")]
        public string Password { get; set; }


        [Required(ErrorMessage = "!Введите пароль еще раз!")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "!Длина пароля должна быть от 6 до 20 символов")]
        [Display(Name = "Введите пароль еще раз")]
        [Compare("Password",ErrorMessage ="!Пароли не совпадают!")]
        public string RePassword { get; set; }


        public DateTime RegistrationDate { get; set; }
        public string Flag { get; set; }
    }
}
