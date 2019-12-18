using System.Collections.Generic;
using System.Text;

namespace QWERTYShop.Models
{
    public static class Parser
    {
        public static string ParseCartElements(List<CartModels> cartElements)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < cartElements.Count; i++)
                if (cartElements.Count == 1)
                    builder.Append(cartElements[i].Id + ":" + cartElements[i].Count);
                else if (i == cartElements.Count - 1)
                    builder.Append(cartElements[i].Id + ":" + cartElements[i].Count);
                else
                    builder.Append(cartElements[i].Id + ":" + cartElements[i].Count + ",");
            return builder.ToString();
        }
    }
}