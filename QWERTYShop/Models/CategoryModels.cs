namespace QWERTYShop.Models
{
    public class CategoryModels
    {
        public string Type { get; set; }
        public string Method { get; set; }
        public bool IsDescending { get; set; }
        public int FromPrice { get; set; }
        public int ToPrice { get; set; }
        public int FromRating { get; set; }
        public int ToRating { get; set; }
    }
}