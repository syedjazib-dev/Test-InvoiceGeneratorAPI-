using InvoiceGenerator.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceGenerator.Utility.PDFService
{
    public interface IPDFService
    {
        public byte[] GenerateApprovalPDF(ApprovalPDF approvalPDF);
        public byte[] GenerateInvoicePDF(InvoicePDF invoicePDF);
    }
}
