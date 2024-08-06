using InvoiceGenerator.Model.Models;

namespace InvoiceGenerator.DataAccess.Repository.IRepository
{
    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        public void Update(RefreshToken refreshToken);
    }
}
