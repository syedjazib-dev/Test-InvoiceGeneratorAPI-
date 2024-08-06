using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InvoiceGenerator.StaticData.Status;
using InvoiceGenrator.Model.Models;

namespace InvoiceGenerator.Model.Models.DTOs
{
    public class ItemCreateDTO
    {
        public int? ParentItemId { get; set; }

        public int? ApprovalId { get; set; }

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

        [Required]
        public string Status { get; set; } = ItemStatus.Pending;

        public List<int>? NewInvoiceIds { get; set; }

        [NotMapped]
        public List<Item>? SplittedItems { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    }
}
