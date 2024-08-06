using InvoiceGenrator.Model.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceGenerator.Model.Models
{
    public class InvoicePDF
    {
        [Required]
        public Invoice Invoice { get; set; }
        [Required]
        public List<Item> Items { get; set; }
        [Required]  
        public string Reciver {get;set;}
        [Required]
        public string salesman { get;set;}
        [Required]
        public string Remarks {get;set;}
        [Required]
        public string Terms { get; set; }
    }
}
