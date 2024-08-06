using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceGenerator.Model.Models.DTOs
{
    public class CustomerCreateDTO
    {

        [Required]
        public string Name { get; set; }

        public string? Email { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string Phone { get; set; }

        public string? TRN { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    }
}
