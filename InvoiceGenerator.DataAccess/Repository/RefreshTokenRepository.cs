using Azure;
using InvoiceGenerator.DataAccess.DbContext;
using InvoiceGenerator.DataAccess.Repository.IRepository;
using InvoiceGenerator.Model.Models;

namespace InvoiceGenerator.DataAccess.Repository
{
    public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
    {

        private readonly ApplicationDbContext _db;
        public RefreshTokenRepository(ApplicationDbContext db) : base(db)
        {
            _db  = db;
        }

        public void Update(RefreshToken refreshToken)
        {
            _db.RefreshTokens.Update(refreshToken);
        }
    }
}
