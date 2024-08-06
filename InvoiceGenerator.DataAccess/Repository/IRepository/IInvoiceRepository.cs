using Azure.Core;
using InvoiceGenerator.Model.Models.DTOs;
using InvoiceGenrator.Model.Models;
using System.Linq.Expressions;

namespace InvoiceGenerator.DataAccess.Repository.IRepository
{
    public interface IInvoiceRepository : IRepository<Invoice>
    {
        public void Update(Invoice invoice);

        public Task<RecordsResponse> GetAllWithSearchAndFilterAsync(string? status, int? customerId, DateTime? createDate = null, string? search = null, Expression<Func<Invoice, string>>? OrderBy = null, string? IncludeProperties = null, string Order = StaticData.Order.ASC, int PageSize = 0, int PageNo = 1);
    }
}
