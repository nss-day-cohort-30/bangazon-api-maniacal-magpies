using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BangazonAPI.Models
{
    /// <summary>
    /// Product: A class describing an Employee
    /// Author: Antonio Jefferson
    /// </summary>
    public class Employee
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [Required]
        public bool IsSuperVisor { get; set; }

        public Department Department { get; set; }

        public Computer Computer { get; set; }

        public List<TrainingProgram> TrainingPrograms { get; set; }
        

    }
}
