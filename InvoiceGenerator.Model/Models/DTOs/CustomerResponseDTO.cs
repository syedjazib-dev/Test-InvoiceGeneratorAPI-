using System.ComponentModel.DataAnnotations;

namespace InvoiceGenerator.Model.Models.DTOs
{
    public class CustomerResponseDTO
    {

        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string? Email { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string Phone { get; set; }

        public string? TRN { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
