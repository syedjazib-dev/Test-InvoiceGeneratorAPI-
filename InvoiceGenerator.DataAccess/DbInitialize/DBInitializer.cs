

using InvoiceGenerator.DataAccess.DbContext;
using InvoiceGenerator.Model.Models.DTOs;
using InvoiceGenerator.StaticData;
using InvoiceGenrator.Model.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InvoiceGenerator.DataAccess.DbInitialize
{
    public class DBInitializer : IDbinitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        public DBInitializer(ApplicationDbContext db, RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public void Initialize()
        {
            //Add Migration if panding
            try
            {
                if(_db.Database.GetPendingMigrations().Count() > 0) { 
                    _db.Database.Migrate(); 
                }
            }catch(Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            //Create role of not exist
            if (!_roleManager.RoleExistsAsync(UserRole.Admin).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(UserRole.Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(UserRole.Salesman)).GetAwaiter().GetResult();


                //Create Admin User
                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@admin.com",
                    Email = "admin@admin.com",
                    Name  = "Admin",
                    PhoneNumber = "123456123",
                    CreateDate = DateTime.UtcNow,
                    IsActive = true,
                    Role = UserRole.Admin,

                }, "Admin@123").GetAwaiter().GetResult();

                ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "admin@admin.com");
                _userManager.AddToRoleAsync(user, UserRole.Admin).GetAwaiter().GetResult();
            }

        }
    }
}
