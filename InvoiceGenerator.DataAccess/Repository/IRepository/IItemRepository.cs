using Azure.Core;
using InvoiceGenerator.Model.Models.DTOs;
using InvoiceGenrator.Model.Models;
using System.Linq.Expressions;

namespace InvoiceGenerator.DataAccess.Repository.IRepository
{
    public interface IItemRepository : IRepository<Item>
    {
        public void Update(Item item);

        public void UpdateRange(List<Item> ListOfentity);

        public Task<RecordsResponse> GetAllWithSearchAndFilterAsync(int? invoiceId = null, int? approvalId = null, List<string>? statusToExclude = null, DateTime? createDate = null, string? search = null, Expression<Func<Item, string>>? OrderBy = null, string? IncludeProperties = null, string Order = StaticData.Order.ASC);
    }
}
