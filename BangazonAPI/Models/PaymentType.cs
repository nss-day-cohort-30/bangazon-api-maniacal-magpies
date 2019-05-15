using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BangazonAPI.Models
{
    //This class provides the model for paymentTypes. Added the customer object on because each payment type is associated with a customer
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
