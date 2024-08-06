using Azure.Core;
using InvoiceGenerator.Model.Models.DTOs;
using InvoiceGenrator.Model.Models;
using System.Linq.Expressions;

namespace InvoiceGenerator.DataAccess.Repository.IRepository
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        public void Update(Customer customer);

        public Task<RecordsResponse> GetAllWithSearchAndFilterAsync(DateTime? createDate = null, string? search = null, Expression<Func<Customer, string>>? OrderBy = null, string? IncludeProperties = null, string Order = StaticData.Order.ASC, int PageSize = 0, int PageNo = 1);
    }
}
