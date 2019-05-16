﻿using System.ComponentModel.DataAnnotations;

namespace BangazonAPI.Models
{
    /// <summary>
    /// Product: A class describing a Product
    /// Author: Antonio Jefferson
    /// </summary>
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public int ProductTypeId { get; set; }

        public string ProductType { get; set; }

        [Required]
        public int CustomerId { get; set; }

        public Customer Customer { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public int Quantity { get; set; }
    }
}
