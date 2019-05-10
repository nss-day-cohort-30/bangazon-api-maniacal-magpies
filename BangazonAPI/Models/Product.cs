using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BangazonAPI.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string ProductType { get; set; }

        public int CustomerId { get; set; }

        public int ProductTypeId { get; set; }

        [Required]
        public Customer Customer { get; set; }

        [Required]
        public double Price { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public int Quantity { get; set; }
    }
}
