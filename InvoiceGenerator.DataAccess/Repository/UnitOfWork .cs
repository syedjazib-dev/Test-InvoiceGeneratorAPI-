using InvoiceGenerator.DataAccess.DbContext;
using InvoiceGenerator.DataAccess.Repository.IRepository;
using InvoiceGenerator.DataAccess.Repository.IRepositoy;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace InvoiceGenerator.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        public IApplicationUserRepository ApplicationUserRepository { get; private set; }
        public IRefreshTokenRepository RefreshTokenRepository { get; private set; }
        public ICustomerRepository CustomerRepository { get; private set; }
        public IApprovalRepository ApprovalRepository { get; private set; }
        public IInvoiceRepository InvoiceRepository { get; private set; }
        public IItemRepository ItemRepository { get; private set; }
        public IInvoiceApprovalRepository InvoiceApprovalRepository { get; private set; }
        public IInvoiceItemRepository InvoiceItemRepository { get; private set; }

        private readonly ApplicationDbContext _db;

        public UnitOfWork(ApplicationDbContext db, IConfiguration configuration, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            ApplicationUserRepository = new ApplicationUserRepository(db, configuration, userManager, roleManager);
            RefreshTokenRepository = new RefreshTokenRepository(db);
            CustomerRepository = new CustomerRepository(db);
            ApprovalRepository = new ApprovalRepository(db);
            InvoiceRepository = new InvoiceRepository(db);
            ItemRepository = new ItemRepository(db);
            InvoiceApprovalRepository = new InvoiceApprovalRepository(db);
            InvoiceItemRepository = new InvoiceItemRepository(db);
        }   

        public void Save()
        {
                _db.SaveChanges();
        }
    }
}
