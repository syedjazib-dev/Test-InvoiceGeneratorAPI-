using InvoiceGenrator.Model.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceGenerator.Model.Models.DTOs
{
    public  class InvoiceItemResponseDTO
    {
        [Key]
        public int Id { get; set; }

        public int ItemId { get; set; }
        public Item Item { get; set; }

        public int InvoiceId { get; set; }
    }
}
