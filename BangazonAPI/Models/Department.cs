using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BangazonAPI.Models
{
    //This class provides the modl for departments. Added th employee list on in order to access employees from department. Employee holds the foreign ky for department
    public class Department
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int Budget { get; set; }

        //added list to allow departments to display all employees
        public List<Employee> Employees { get; set; } 


    }
}
