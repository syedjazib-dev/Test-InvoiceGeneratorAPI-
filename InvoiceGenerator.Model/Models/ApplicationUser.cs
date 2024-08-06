using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceGenrator.Model.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Role { get; set; }

        [Required]
        public bool IsActive { get; set; }
           
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }

    }
}
