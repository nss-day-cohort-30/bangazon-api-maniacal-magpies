using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BangazonAPI.Models
{
    /// <summary>
    /// TrainingProgram: A class describing a Training Program at Bangazon
    /// Author: Antonio Jefferson
    /// </summary>
    public class TrainingProgram
    {
        public int Id { get; set; }  

        [Required]
        public string Name { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public int MaxAttendees { get; set; }

        public List<Employee> Employees { get; set; }

    }
}
