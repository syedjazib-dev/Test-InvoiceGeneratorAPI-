using System.Linq.Expressions;

namespace InvoiceGenerator.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        public Task<T> GetAsync(Expression<Func<T, bool>>? filter = null, bool Trecked = true, string? IncludeProperties = null, string? ThenIncludeProperties = null);
        public Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, Expression<Func<T, string>>? OrderBy = null, string? IncludeProperties = null, string? ThenIncludeProperties = null, string Order = StaticData.Order.ASC, int PageSize = 0, int PageNo = 1);

        public Task<T> CreateAsync(T entity);

        public Task<List<T>> CreateRangeAsync(List<T> ListyOfentity);

        public Task RemoveAsync(T entity);  

        public Task RemoveRangeAsync(List<T> ListOfentity);

        public Task SaveAsync();
        public Expression<Func<T, string>> CreateSelectorExpression(string propertyName);
    }
}
