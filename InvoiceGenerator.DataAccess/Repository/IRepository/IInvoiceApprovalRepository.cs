using Azure.Core;
using InvoiceGenerator.Model.Models;
using InvoiceGenerator.Model.Models.DTOs;
using InvoiceGenrator.Model.Models;
using System.Linq.Expressions;

namespace InvoiceGenerator.DataAccess.Repository.IRepository
{
    public interface IInvoiceApprovalRepository : IRepository<InvoiceApproval>
    {
        public void Update(InvoiceApproval entity);

        public void UpdateRange(List<InvoiceApproval> ListOfentity);

    }
}
