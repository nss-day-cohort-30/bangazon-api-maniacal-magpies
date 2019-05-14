﻿using System;
using System.ComponentModel.DataAnnotations;

namespace BangazonAPI.Models
{
    public class Computer
    {
        public int Id { get; set; }

        [Required]
        public DateTime PurchaseDate { get; set; }

        
        public DateTime DecomissionDate { get; set; }

        [Required]
        public string Make { get; set; }

        [Required]
        public string Manufacturer { get; set; }

        public DateTime AssignDate { get; set; }

        public DateTime UnassignDate { get; set; }
    }
}
