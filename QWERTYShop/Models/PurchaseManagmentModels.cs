namespace QWERTYShop.Models
{
    public class PurchaseManagmentModels
    {
        public string Condition { get; set; }

        public int Id { get; set; }

        public override string ToString()
        {
            string condition = "";
            switch (Condition)
            {
                case "handles":
                    condition = "Обрабатывается";
                    break;
                case "delivers":
                    condition = "Доставляется до клиента/ доставляется до пункта выдачи заказа";
                    break;
                case "ready to pickup":
                    condition = "Доставлен до пункта выдачи заказа и готов к получению";
                    break;
                default: condition = "Доставлен";
                    break;
            }
            return $"Id товара:={Id} Текущее состояние:={condition}";
        }
    }
}