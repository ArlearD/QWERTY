namespace QWERTYShop.Models
{
    public class PurchaseManagmentModels
    {
        public string Condition { get; set; }

        public int Id { get; set; }

        public override string ToString()
        {
            return $"Id:={Id} Condition:={Condition}";
        }
    }
}