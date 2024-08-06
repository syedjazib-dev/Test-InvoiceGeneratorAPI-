using InvoiceGenerator.DataAccess.DbContext;
using InvoiceGenerator.DataAccess.Repository.IRepository;
using InvoiceGenrator.Model.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using InvoiceGenerator.DataAccess.Predicate;
using InvoiceGenerator.Model.Models;

namespace InvoiceGenerator.DataAccess.Repository
{
    public class InvoiceApprovalRepository : Repository<InvoiceApproval>, IInvoiceApprovalRepository
    {

        private readonly ApplicationDbContext _db;
        public InvoiceApprovalRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(InvoiceApproval entity)
        {
            _db.InvoiceApprovals.Update(entity);
        }

        public void UpdateRange(List<InvoiceApproval> ListOfentity)
        {
            _db.InvoiceApprovals.UpdateRange(ListOfentity);
        }

    }
}
