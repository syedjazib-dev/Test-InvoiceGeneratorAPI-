using InvoiceGenerator.Model.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceGenrator.Model.Models
{
    public class Invoice
    {
        [Key]
        public int Id { get; set; }

        public string? InvoiceNo { get; set; }

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

        [Required]
        public string Status { get; set; }

        public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();

        public ICollection<InvoiceApproval> InvoiceApprovals { get; set; } = new List<InvoiceApproval>();

        public DateTime? PaymentDate { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
