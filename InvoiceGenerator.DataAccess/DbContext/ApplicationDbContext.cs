using InvoiceGenerator.Model.Models;
using InvoiceGenrator.Model.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InvoiceGenerator.DataAccess.DbContext
{
    public  class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Approval> Approvals { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<InvoiceApproval> InvoiceApprovals { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
    }
}
