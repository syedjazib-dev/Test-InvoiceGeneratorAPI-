using InvoiceGenerator.DataAccess.DbContext;
using InvoiceGenerator.DataAccess.Repository.IRepository;
using InvoiceGenrator.Model.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using InvoiceGenerator.DataAccess.Predicate;

namespace InvoiceGenerator.DataAccess.Repository
{
    public class ItemRepository : Repository<Item>, IItemRepository
    {

        private readonly ApplicationDbContext _db;
        public ItemRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Item entity)
        {
            _db.Items.Update(entity);
        }

        public void UpdateRange(List<Item> ListOfentity)
        {
            _db.Items.UpdateRange(ListOfentity);
        }

        public async Task<RecordsResponse> GetAllWithSearchAndFilterAsync(int? invoiceId = null, int? approvalId = null, List<string>? statusToExclude = null, DateTime? createDate = null, string? search = null, Expression<Func<Item, string>>? OrderBy = null, string? IncludeProperties = null, string Order = StaticData.Order.ASC)
        {
            IQueryable<Item> query = dbSet;
            var predicate = PredicateBuilder.True<Item>();
            if (statusToExclude != null && statusToExclude.Count > 0)
            {
                statusToExclude.ForEach(excludeStatus => {
                    predicate = predicate.And(u => u.Status != excludeStatus);
                });
            }
            if (approvalId != null)
            {
                predicate = predicate.And(u => u.ApprovalId == approvalId);
            }
            if (createDate.HasValue)
            {
                DateTime filterCreateDate = createDate.Value;
                predicate = predicate.And(u => DateTime.Compare(u.CreateDate.Date, filterCreateDate) == 0);
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
            if (IncludeProperties != null)
            {
                foreach (var IncludeProp in IncludeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var prop = IncludeProp.Trim();
                    query = query.Include(prop);
                }
            }
            IEnumerable<Item> List = await query.ToListAsync();

            return new RecordsResponse()
            {
                Records = List,
                TotalRecords = totalrecords,
            };
        }
    }
}
