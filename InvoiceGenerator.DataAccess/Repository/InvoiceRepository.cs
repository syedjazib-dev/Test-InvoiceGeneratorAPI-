using InvoiceGenerator.DataAccess.DbContext;
using InvoiceGenerator.DataAccess.Repository.IRepository;
using InvoiceGenrator.Model.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using InvoiceGenerator.DataAccess.Predicate;

namespace InvoiceGenerator.DataAccess.Repository
{
    public class InvoiceRepository : Repository<Invoice>, IInvoiceRepository
    {

        private readonly ApplicationDbContext _db;
        public InvoiceRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Invoice entity)
        {
            _db.Invoices.Update(entity);
        }


        public async Task<RecordsResponse> GetAllWithSearchAndFilterAsync(string? status, int? customerId, DateTime? createDate = null, string? search = null, Expression<Func<Invoice, string>>? OrderBy = null, string? IncludeProperties = null, string Order = StaticData.Order.DESC, int PageSize = 0, int PageNo = 1)
        {
            IQueryable<Invoice> query = dbSet;
            var predicate = PredicateBuilder.True<Invoice>();
            if (!string.IsNullOrEmpty(status))
            {
                predicate = predicate.And(u => u.Status == status);
            }
            if (customerId != null)
            {
                predicate = predicate.And(u => u.CustomerId == customerId);
            }
            if (createDate.HasValue)
            {
                DateTime filterCreateDate = createDate.Value;
                predicate = predicate.And(u => DateTime.Compare(u.CreateDate.Date, filterCreateDate) == 0);
            }
            if (!string.IsNullOrEmpty(search))
            {
                predicate = predicate.And(p => p.InvoiceNo.ToLower().Contains(search) || p.Customer.Name.ToLower().Contains(search));
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
            IEnumerable<Invoice> List = await query.ToListAsync();

            return new RecordsResponse()
            {
                Records = List,
                TotalRecords = totalrecords,
            };
        }
    }
}
