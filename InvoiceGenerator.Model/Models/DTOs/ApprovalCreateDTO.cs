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
    public class ApprovalCreateDTO
    {
        [Required]
        public double TotalAmount { get; set; }
        [Required]
        public double Vat { get; set; }
        [Required]
        public double TotalAmountIncludingVat { get; set; }
        [Required]
        public int CustomerId { get; set; }

        [Required]
        public string SalesmanId { get; set; }

        [Required]
        public string Status { get; set; }  = ApprovalStatus.Pending;

        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    }
}
