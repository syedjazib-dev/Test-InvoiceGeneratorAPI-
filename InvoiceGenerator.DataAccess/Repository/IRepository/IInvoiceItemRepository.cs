using Azure.Core;
using InvoiceGenerator.Model.Models;
using InvoiceGenerator.Model.Models.DTOs;
using InvoiceGenrator.Model.Models;
using System.Linq.Expressions;

namespace InvoiceGenerator.DataAccess.Repository.IRepository
{
    public interface IInvoiceItemRepository : IRepository<InvoiceItem>
    {
        public void Update(InvoiceItem entity);

        public void UpdateRange(List<InvoiceItem> ListOfentity);

    }
}
