using System.Collections.Generic;

namespace BangazonAPI.Models
{
    /// <summary>
    /// Product: A class describing an Order
    /// Author: Antonio Jefferson
    /// </summary>
    public class Order
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public Customer Customer { get; set; }

        public int? PaymentTypeId { get; set; }

        public PaymentType Payment { get; set; }

        public List<Product> Products { get; set; }
    }
}
