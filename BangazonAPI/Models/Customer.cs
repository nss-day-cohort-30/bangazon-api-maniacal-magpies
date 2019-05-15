using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BangazonAPI.Models
{
    /// <summary>
    /// Product: A class describing a Customer
    /// Author: Antonio Jefferson
    /// </summary>
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public List<Product> ProductsSelling { get; set; }
        public List<PaymentType> PaymentTypesUsed { get; set; }
    }
}