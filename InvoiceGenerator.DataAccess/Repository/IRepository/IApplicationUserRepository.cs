using Azure.Core;
using InvoiceGenerator.Model.Models.DTOs;
using InvoiceGenrator.Model.Models;
using System.Linq.Expressions;

namespace InvoiceGenerator.DataAccess.Repository.IRepository
{
    public interface IApplicationUserRepository : IRepository<ApplicationUser>
    {
        public void Update(ApplicationUser applicationUser);

        Task<bool> IsUniqueUser(string email);

        public Task Register(UserCreateRequestDTO userCreateRequestDTO);

        public Task<UserLoginResponseDTO> Login(UserLoginRequest userLoginRequest);
        public Task<UserLoginResponseDTO> LoginMobile(UserLoginRequest userLoginRequest);

        public Task<TokenDTO> RefreshToken (string refreshToken);

        public Task<RecordsResponse> GetAllWithSearchAndFilterAsync(string? role = null, bool? isActive = null, DateTime? createDate = null, string? search = null, Expression<Func<ApplicationUser, string>>? OrderBy = null, string? IncludeProperties = null, string Order = StaticData.Order.ASC, int PageSize = 0, int PageNo = 1);
    }
}
