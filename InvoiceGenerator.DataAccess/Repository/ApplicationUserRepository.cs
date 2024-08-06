using Azure;
using InvoiceGenerator.DataAccess.DbContext;
using InvoiceGenerator.DataAccess.Repository.IRepository;
using InvoiceGenerator.DataAccess.Repository.IRepositoy;
using InvoiceGenerator.Model.Models;
using InvoiceGenerator.Model.Models.DTOs;
using InvoiceGenerator.StaticData;
using InvoiceGenrator.Model.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using InvoiceGenerator.DataAccess.Predicate;

namespace InvoiceGenerator.DataAccess.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {

        private readonly ApplicationDbContext _db;

        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        public ApplicationUserRepository(ApplicationDbContext db, IConfiguration configuration, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager) : base(db)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public void Update(ApplicationUser applicationUser)
        {
            _db.ApplicationUsers.Update(applicationUser);
        }

        public async Task<bool> IsUniqueUser(string email)
        {
            ApplicationUser? user = await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return true;
            }
            return false;
        }

        public async Task Register(UserCreateRequestDTO userCreateRequestDTO)
        {

            var role = _roleManager.FindByNameAsync(userCreateRequestDTO.Role);
            if (role == null)
            {
                userCreateRequestDTO.Role = UserRole.Salesman;
            }
            var result = await _userManager.CreateAsync(new ApplicationUser
            {
                UserName = userCreateRequestDTO.Email,
                Email = userCreateRequestDTO.Email,
                Name = userCreateRequestDTO.Name,
                IsActive = true,
                Role = userCreateRequestDTO.Role,
                CreateDate = userCreateRequestDTO.CreateDate,
            }, userCreateRequestDTO.Password);

            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.Select(e => e.Description).First());
            }
            var user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == userCreateRequestDTO.Email);

            if (role != null)
            {
                await _userManager.AddToRoleAsync(user, userCreateRequestDTO.Role);
            }
            else
            {
                await _userManager.AddToRoleAsync(user, UserRole.Salesman);
            }
        }

        public async Task<UserLoginResponseDTO> Login(UserLoginRequest userLoginRequest)
        {
            var user = await _userManager.FindByEmailAsync(userLoginRequest.Email);
            if (user == null)
            {
                throw new Exception("Email is not registered.");
            }

            var userFormDb = await GetAsync(u => u.Id == user.Id);
            if (userFormDb == null)
            {
                throw new Exception("Email is not registered.");
            }

            if (!userFormDb.IsActive)
            {
                throw new Exception("Your account is disabled by administrator.");
            }

            var result = await _userManager.CheckPasswordAsync(user, userLoginRequest.Password);

            if (!result)
            {
                throw new Exception("Invalid Password");
            }

            return new UserLoginResponseDTO
            {
                UserId = user.Id,
                TokenDTO = await GenerateAccessAndRefreshToken(user)
            };
        }

        public async Task<UserLoginResponseDTO> LoginMobile(UserLoginRequest userLoginRequest)
        {
            var user = await _userManager.FindByEmailAsync(userLoginRequest.Email);
            if (user == null)
            {
                throw new Exception("Email is not registered.");
            }

            var userFormDb = await GetAsync(u => u.Id == user.Id);
            if (userFormDb == null)
            {
                throw new Exception("Email is not registered.");
            }

            if (!userFormDb.IsActive)
            {
                throw new Exception("Your account is disabled by administrator.");
            }

            var result = await _userManager.CheckPasswordAsync(user, userLoginRequest.Password);

            if (!result)
            {
                throw new Exception("Invalid Password");
            }

            return new UserLoginResponseDTO
            {
                UserId = user.Id,
                TokenDTO = await GenerateAccessOnly(user)
            };
        }


        private async Task<TokenDTO> GenerateAccessAndRefreshToken(IdentityUser user)
        {
            // Create new Access token
            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JTWSettings:SecretKey"]));

            var JwtToken = new JwtSecurityToken(
                    issuer: _configuration["JTWSettings:Issuer"],
                    audience: _configuration["JTWSettings:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(15),
                    signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
                );



            //Create new refresh token
            var randomNumber = new Byte[32];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);


            var refreshToken = new RefreshToken()
            {
                UserId = user.Id,
                IsRevoked = false,
                CreatedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                Token = Convert.ToBase64String(randomNumber) + Guid.NewGuid()
            };

            _db.RefreshTokens.Add(refreshToken);
            _db.SaveChanges();

            string AccessTokenAsString = new JwtSecurityTokenHandler().WriteToken(JwtToken);

            return new TokenDTO
            {
                AccessToken = AccessTokenAsString,
                RefreshToken = refreshToken.Token
            };
        }

        private async Task<TokenDTO> GenerateAccessOnly(IdentityUser user)
        {
            // Create new Access token
            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JTWSettings:SecretKey"]));

            var JwtToken = new JwtSecurityToken(
                    issuer: _configuration["JTWSettings:Issuer"],
                    audience: _configuration["JTWSettings:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddDays(7),
                    signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
                );


            string AccessTokenAsString = new JwtSecurityTokenHandler().WriteToken(JwtToken);

            return new TokenDTO
            {
                AccessToken = AccessTokenAsString,
                RefreshToken = ""
            };
        }

        public async Task<TokenDTO> RefreshToken(string refreshToken)
        {
            //Check refresh token exist or nor
            var RefreshTokenFromDb = _db.RefreshTokens.FirstOrDefault(u => u.Token == refreshToken);
            if (RefreshTokenFromDb == null)
            {
                throw new Exception("Invalid Refersh Token");
            }

            // Check token revocked 
            if (RefreshTokenFromDb.IsRevoked)
            {
                _db.RefreshTokens.Remove(RefreshTokenFromDb);
                _db.SaveChanges();
                throw new Exception("Token has been revocked");
            }

            // Check token is expired
            if (RefreshTokenFromDb.ExpiryDate <= DateTime.UtcNow)
            {
                _db.RefreshTokens.Remove(RefreshTokenFromDb);
                _db.SaveChanges();
                throw new Exception("Token has been expired");
            }

            // RemoveUsed Token And generate new 

            var user = await _userManager.FindByIdAsync(RefreshTokenFromDb.UserId);
            if (user == null)
            {
                throw new Exception("Invalid Refersh Token");
            }
            _db.RefreshTokens.Remove(RefreshTokenFromDb);
            _db.SaveChanges();

            TokenDTO tokenDTO = await GenerateAccessAndRefreshToken(user);

            return tokenDTO;
        }



        public async Task<RecordsResponse> GetAllWithSearchAndFilterAsync(string? role = null, bool? isActive = null, DateTime? createDate = null, string? search = null, Expression<Func<ApplicationUser, string>>? OrderBy = null, string? IncludeProperties = null, string Order = StaticData.Order.ASC, int PageSize = 0, int PageNo = 1)
        {
            IQueryable<ApplicationUser> query = dbSet;
            var predicate = PredicateBuilder.True<ApplicationUser>();
            if (role != null)
            {
                predicate = predicate.And(u => u.Role == role);
            }
            if (isActive != null)
            {
                predicate = predicate.And(u => u.IsActive == isActive);
            }
            if (createDate.HasValue)
            {
                DateTime filterCreateDate = createDate.Value;
                predicate = predicate.And(u => DateTime.Compare(u.CreateDate.Date, filterCreateDate) == 0);
            }
            if (!string.IsNullOrEmpty(search))
            {
                predicate = predicate.And(p => p.Name.ToLower().Contains(search) || p.Email.ToLower().Contains(search));
            }
            query = query.Where(predicate);

            if (OrderBy != null)
            {
                if (Order == StaticData.Order.DESC)
                {
                    query = query.OrderByDescending(OrderBy);
                }
                else
                {
                    query = query.OrderBy(OrderBy);
                }
            }

            var totalrecords = (await query.ToListAsync()).Count();
            if (PageSize > 0)
            {
                if (PageSize > 100)
                {
                    PageSize = 100;
                }
                query = query.Skip(PageSize * (PageNo - 1)).Take(PageSize);
            }
            if (IncludeProperties != null)
            {
                foreach (var IncludeProp in IncludeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var prop = IncludeProp.Trim();
                    query = query.Include(prop);
                }
            }
            IEnumerable<ApplicationUser> List = await query.ToListAsync();

            return new RecordsResponse()
            {
                Records = List,
                TotalRecords = totalrecords,
            };
        }
    }
}
