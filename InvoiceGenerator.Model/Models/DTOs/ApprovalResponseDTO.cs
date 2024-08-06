using InvoiceGenrator.Model.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceGenerator.Model.Models.DTOs
{
    public class ApprovalResponseDTO
    {
        public int Id { get; set; }

        [Required]
        public string ApprovalNo { get; set; }

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
        public UserResponseDTO Salesman { get; set; }

        [Required]
        public string Status { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
