using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceGenerator.Model.Models.DTOs
{
    public class UserUpdateDTO
    {

        [Required]
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Role { get; set; }

        public DateTime UpdateDate { get; set; } = DateTime.UtcNow;
    }
}
