using InvoiceGenerator.DataAccess.DbContext;
using InvoiceGenerator.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace InvoiceGenerator.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        internal DbSet<T> dbSet;
        public Repository(ApplicationDbContext db)
        {
            _db = db;
            this.dbSet = _db.Set<T>();

        }

        public async Task<T> CreateAsync(T entity)
        {
            await dbSet.AddAsync(entity);
            await SaveAsync();
            return entity;
        }

        public async Task<List<T>> CreateRangeAsync(List<T> ListyOfentity)
        {
            await dbSet.AddRangeAsync(ListyOfentity);
            await SaveAsync();
            return ListyOfentity;
        }

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, Expression<Func<T, string>>? OrderBy = null, string? IncludeProperties = null, string ? ThenIncludeProperties = null, string Order = StaticData.Order.ASC, int PageSize = 0, int PageNo = 1)
        {
            IQueryable<T> query = dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            if (OrderBy != null)
            {
                if(Order == StaticData.Order.DESC)
                {
                    query = query.OrderByDescending(OrderBy);
                }
                else
                {
                    query = query.OrderBy(OrderBy);
                }
            }
            if (PageSize > 0)
            {
                query = query.Skip(PageSize * (PageNo - 1)).Take(PageSize);
            }
            if (IncludeProperties != null)
            {
                foreach (var IncludeProp in IncludeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var prop = IncludeProp.Trim();
                    query = query.Include(prop);
                }
                if (ThenIncludeProperties != null)
                {
                    foreach (var IncludeProp in ThenIncludeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var prop = IncludeProp.Trim();
                        query = query.Include(prop);
                    }
                }
            }
            return await query.ToListAsync();
        }

        public async Task<T> GetAsync(Expression<Func<T, bool>>? filter = null, bool Trecked = true, string? IncludeProperties = null, string? ThenIncludeProperties = null)
        {
            IQueryable<T> query = dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            if (!Trecked)
            {
                query = query.AsNoTracking<T>();
            }
            if (IncludeProperties != null)
            {
                foreach (var IncludeProp in IncludeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var prop = IncludeProp.Trim();
                    query = query.Include(prop);
                }
                if (ThenIncludeProperties != null)
                {
                    foreach (var IncludeProp in ThenIncludeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var prop = IncludeProp.Trim();
                        query = query.Include(prop);
                    }
                }
            }
            return await query.FirstOrDefaultAsync();
        }

        public async Task RemoveAsync(T entity)
        {
            dbSet.Remove(entity);
            await SaveAsync();
        }

        public async Task RemoveRangeAsync(List<T> ListOfentity)
        {
            dbSet.RemoveRange(ListOfentity);
            await SaveAsync();
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }

        public Expression<Func<T, string>> CreateSelectorExpression(string propertyName)
        {
            var paramterExpression = Expression.Parameter(typeof(T));
            return (Expression<Func<T, string>>)Expression.Lambda(Expression.PropertyOrField(paramterExpression, propertyName),
                                                                    paramterExpression);
        }
    }
}
