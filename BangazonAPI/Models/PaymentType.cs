using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BangazonAPI.Models
{
    public class PaymentType
    {
        public int Id { get; set; }

        [Required]
        public string AcctNumber { get; set; }

        [Required]
        public string Name { get; set; }

        public int CustomerId { get; set; }

        public Customer Customer { get; set; }
    }
}
