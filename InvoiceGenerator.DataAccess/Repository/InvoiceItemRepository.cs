using InvoiceGenerator.DataAccess.DbContext;
using InvoiceGenerator.DataAccess.Repository.IRepository;
using InvoiceGenerator.Model.Models;

namespace InvoiceGenerator.DataAccess.Repository
{
    public class InvoiceItemRepository : Repository<InvoiceItem>, IInvoiceItemRepository
    {

        private readonly ApplicationDbContext _db;
        public InvoiceItemRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(InvoiceItem entity)
        {
            _db.InvoiceItems.Update(entity);
        }

        public void UpdateRange(List<InvoiceItem> ListOfentity)
        {
            _db.InvoiceItems.UpdateRange(ListOfentity);
        }

    }
}
