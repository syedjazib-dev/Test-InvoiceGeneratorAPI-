using InvoiceGenerator.DataAccess.DbContext;
using InvoiceGenerator.DataAccess.Repository.IRepository;
using InvoiceGenrator.Model.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using InvoiceGenerator.DataAccess.Predicate;

namespace InvoiceGenerator.DataAccess.Repository
{
    public class CustomerRepository : Repository<Customer>, ICustomerRepository
    {

        private readonly ApplicationDbContext _db;
        public CustomerRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Customer customer)
        {
            _db.Customers.Update(customer);
        }


        public async Task<RecordsResponse> GetAllWithSearchAndFilterAsync(DateTime? createDate = null, string? search = null, Expression<Func<Customer, string>>? OrderBy = null, string? IncludeProperties = null, string Order = StaticData.Order.ASC, int PageSize = 0, int PageNo = 1)
        {
            IQueryable<Customer> query = dbSet;
            var predicate = PredicateBuilder.True<Customer>();
            if (createDate.HasValue)
            {
                DateTime filterCreateDate = createDate.Value;
                predicate = predicate.And(u => DateTime.Compare(u.CreateDate.Date, filterCreateDate) == 0);
            }
            if (!string.IsNullOrEmpty(search))
            {
                predicate = predicate.And(p => p.Name.ToLower().Contains(search) || p.Email.ToLower().Contains(search) || p.Address.ToLower().Contains(search) || p.Phone.ToLower().Contains(search));
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
            IEnumerable<Customer> List = await query.ToListAsync();

            return new RecordsResponse()
            {
                Records = List,
                TotalRecords = totalrecords,
            };
        }
    }
}
