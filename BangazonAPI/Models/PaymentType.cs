using System.ComponentModel.DataAnnotations;

namespace BangazonAPI.Models
{
    /// <summary>
    /// Product: //This class provides the model for paymentTypes. Added the customer object on because each payment type is associated with a customer
    /// Author: Antonio Jefferson
    /// </summary>
    public class PaymentType
    {
        public int Id { get; set; }

        [Required]
        public int AcctNumber { get; set; }

        [Required]
        public string Name { get; set; }

        public int CustomerId { get; set; }

        public Customer Customer { get; set; }
    }
}
