using InvoiceGenerator.DataAccess.Repository.IRepository;

namespace InvoiceGenerator.DataAccess.Repository.IRepositoy
{
    public interface IUnitOfWork
    {
        IApplicationUserRepository ApplicationUserRepository { get; }
        IRefreshTokenRepository RefreshTokenRepository { get; }
        ICustomerRepository CustomerRepository { get; }
        IApprovalRepository ApprovalRepository { get; }
        IInvoiceRepository InvoiceRepository { get; }
        IItemRepository ItemRepository { get; }
        IInvoiceApprovalRepository InvoiceApprovalRepository { get; }
        IInvoiceItemRepository InvoiceItemRepository { get; }
        void Save();
    }
}
