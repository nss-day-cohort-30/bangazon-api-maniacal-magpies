using System.ComponentModel.DataAnnotations;

namespace BangazonAPI.Models
{
    /// <summary>
    /// ProductType: A class describing a Product Type
    /// Author: Antonio Jefferson
    /// </summary>

    public class ProductType
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
