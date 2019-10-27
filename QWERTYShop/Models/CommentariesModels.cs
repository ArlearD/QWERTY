using System;
using System.ComponentModel.DataAnnotations;

namespace QWERTYShop.Models
{
    public class CommentariesModels
    {
        public string UserName { get; set; }

        [Required(ErrorMessage = "Напишите отзыв!")]
        public string Comment { get; set; }
        public DateTime Time { get; set; }
        public long Id { get; set; }
        public int Mark { get; set; }

        public override string ToString()
        {
            return $"{UserName} Дата отправки:{Time.ToString().Split(' ')[0]} Оценка товару:{Mark} \n"+
                   $"Отзыв:{Comment}";
        }
    }
}