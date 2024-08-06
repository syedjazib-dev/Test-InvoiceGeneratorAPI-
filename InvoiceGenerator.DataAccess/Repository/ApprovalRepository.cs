using InvoiceGenerator.DataAccess.DbContext;
using InvoiceGenerator.DataAccess.Repository.IRepository;
using InvoiceGenrator.Model.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using InvoiceGenerator.DataAccess.Predicate;

namespace InvoiceGenerator.DataAccess.Repository
{
    public class ApprovalRepository : Repository<Approval>, IApprovalRepository
    {

        private readonly ApplicationDbContext _db;
        public ApprovalRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Approval entity)
        {
            _db.Approvals.Update(entity);
        }

        public void UpdateRange(List<Approval> ListOfentity)
        {
            _db.Approvals.UpdateRange(ListOfentity);
        }

        public async Task<RecordsResponse> GetAllWithSearchAndFilterAsync(string? status, List<string>? statusToExclude, int? customerId, int? invoiceId , DateTime? createDate = null, string? search = null, Expression<Func<Approval, string>>? OrderBy = null, string? IncludeProperties = null, string Order = StaticData.Order.ASC, int PageSize = 0, int PageNo = 1)
        {
            IQueryable<Approval> query = dbSet;
            var predicate = PredicateBuilder.True<Approval>();
            if (status != null)
            {
                predicate = predicate.And(u => u.Status == status);
            }
            if (customerId != null)
            {
                predicate = predicate.And(u => u.CustomerId== customerId);
            }
            if(statusToExclude != null && statusToExclude.Count > 0)
            {
                statusToExclude.ForEach(excludeStatus => {
                    predicate = predicate.And(u => u.Status != excludeStatus);
                });
            }
            if (createDate.HasValue)
            {
                DateTime filterCreateDate = createDate.Value;
                predicate = predicate.And(u => DateTime.Compare(u.CreateDate.Date, filterCreateDate) == 0);
            }
            if (!string.IsNullOrEmpty(search))
            {
                predicate = predicate.And(p => p.ApprovalNo.ToLower().Contains(search) || p.Customer.Name.ToLower().Contains(search));
            }
            query = query.Where(predicate);

                if (OrderBy != null)
            {
                if (Order == StaticData.Order.DESC)
                {
                    query = query.OrderByDescending(OrderBy);
                }
                else
                {
                    query = query.OrderBy(OrderBy);
                }
            }

            var totalrecords = (await query.ToListAsync()).Count;
            if (PageSize > 0)
            {
                if (PageSize > 100)
                {
                    PageSize = 100;
                }
                query = query.Skip(PageSize * (PageNo - 1)).Take(PageSize);
            }
            if (IncludeProperties != null)
            {
                foreach (var IncludeProp in IncludeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var prop = IncludeProp.Trim();
                    query = query.Include(prop);
                }
            }
            IEnumerable<Approval> List = await query.ToListAsync();

            return new RecordsResponse()
            {
                Records = List,
                TotalRecords = totalrecords,
            };
        }
    }
}
