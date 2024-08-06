using InvoiceGenerator.Model.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceGenrator.Model.Models
{
    public class Item
    {
        [Key]
        public int Id { get; set; }

        public int? ParentItemId { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(ParentItemId))]
        public Item? ParentItem { get; set; }

        public int? ApprovalId { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(ApprovalId))]
        public Approval? Approval { get; set; }

        [Required]
        public string Description { get; set; }

        public string? LotNo { get; set; }

        [Required]
        public double WeightCarats { get; set; }

        [Required]
        public string QuantityUnit { get; set; }

        [Required]
        public double PricePerUnit { get; set; }

        [Required]
        public string PricePerUnitCurrancy{ get; set; }


        [Required]
        public double Amount { get; set; }
        [Required]
        public double Vat { get; set; }
        [Required]
        public double AmountIncludingVat { get; set; }

        [Required]
        public string Status { get; set; }

        public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();

        [NotMapped]
        public List<Item>? SplittedItems { get; set; }

        [NotMapped]
        public List<int>? NewInvoiceIds { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
