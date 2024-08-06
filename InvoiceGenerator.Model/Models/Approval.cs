using InvoiceGenerator.Model.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceGenrator.Model.Models
{
    public class Approval
    {
        [Key]
        public int Id { get; set; }

        public string? ApprovalNo { get; set; }

        [Required]
        public double TotalAmount { get; set; }
        [Required]
        public double Vat { get; set; }
        [Required]
        public double TotalAmountIncludingVat { get; set; }
        [Required]
        public int CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        [ValidateNever]
        public Customer Customer { get; set; }

        [Required]
        public string SalesmanId { get; set; }

        [ForeignKey(nameof(SalesmanId))]
        [ValidateNever]
        public ApplicationUser Salesman { get; set; }

        [ValidateNever]
        public ICollection<InvoiceApproval> InvoiceApprovals { get; set; } = new List<InvoiceApproval>();

        [Required]
        public string Status { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
