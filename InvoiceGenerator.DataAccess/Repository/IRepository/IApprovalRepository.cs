using Azure.Core;
using InvoiceGenerator.Model.Models.DTOs;
using InvoiceGenrator.Model.Models;
using System.Linq.Expressions;

namespace InvoiceGenerator.DataAccess.Repository.IRepository
{
    public interface IApprovalRepository : IRepository<Approval>
    {
        public void Update(Approval approval);

        public void UpdateRange(List<Approval> ListOfentity);

        public Task<RecordsResponse> GetAllWithSearchAndFilterAsync(string? status, List<string>? statusToExclude, int? customerId, int? invoiceId, DateTime? createDate = null, string? search = null, Expression<Func<Approval, string>>? OrderBy = null, string? IncludeProperties = null, string Order = StaticData.Order.ASC, int PageSize = 0, int PageNo = 1);
    }
}
