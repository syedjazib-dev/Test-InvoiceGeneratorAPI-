using InvoiceGenrator.Model.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvoiceGenerator.StaticData.Status;

namespace InvoiceGenerator.Model.Models.DTOs
{
    public class ItemResponseDTO
    {
        [Required]
        public int Id { get; set; }

        public int? ParentItemId { get; set; }
        [ValidateNever]
        [ForeignKey(nameof(ParentItemId))]
        public Item? ItemId { get; set; }

        public int? ApprovalId { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(ApprovalId))]
        public Invoice? Approval { get; set; }

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
        public string PricePerUnitCurrancy { get; set; }

        [Required]
        public double Amount { get; set; }
        [Required]
        public double Vat { get; set; }
        [Required]
        public double AmountIncludingVat { get; set; }

        public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();

        [Required]
        public string Status { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
