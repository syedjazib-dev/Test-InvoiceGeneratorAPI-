using InvoiceGenrator.Model.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceGenerator.Model.Models.DTOs
{
    public  class InvoiceApprovalResponseDTO
    {
        [Key]
        public int Id { get; set; }

        public int ApprovalId { get; set; }
        public Approval Approval { get; set; }

        public int InvoiceId { get; set; }
    }
}
