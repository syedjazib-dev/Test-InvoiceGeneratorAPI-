using AutoMapper;
using ClosedXML.Excel;
using CRM.Models;
using InvoiceGenerator.DataAccess.Repository.IRepositoy;
using InvoiceGenerator.Model.Models.DTOs;
using InvoiceGenerator.StaticData;
using InvoiceGenerator.StaticData.TableFields;
using InvoiceGenrator.Model.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using System.Net;

namespace InvoiceGenratorAPI.Controllers
{
    [Route("api/user")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private APIResponse _response;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(IUnitOfWork unitOfWork, IMapper mapper, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _response = new APIResponse();
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
            _roleManager = roleManager;
        }

        [Authorize]
        [HttpGet("{id}", Name = "get")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> Get(string id)
        {
            try
            {
                if (id == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add("Id not Provided.");
                    return _response;
                }

                var user = await _unitOfWork.ApplicationUserRepository.GetAsync(u => u.Id == id);
                if (user == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages.Add("User not found.");
                    return _response;
                }

                if (!user.IsActive)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add("Your account is disabled by administrator.");
                    return _response;
                }


                UserResponseDTO userResponseDTO = _mapper.Map<UserResponseDTO>(user);

                _response.Data = userResponseDTO;
                _response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                _response.ErrorMessages.Add("Something is wrong.");
                _response.IsSuccess = false;
            }
            return _response;
        }

        [Authorize(Roles = UserRole.Admin)]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> GetAll([FromQuery] string? role, [FromQuery] bool? isActive, [FromQuery] string? createDateMiliSecondsString, [FromQuery] string? search, [FromQuery] string? orderBy, [FromQuery] string? order = Order.ASC, [FromQuery] int PageSize = 0, [FromQuery] int PageNo = 1)
        {
            try
            {
                IEnumerable<ApplicationUser> users = new List<ApplicationUser>();
                RecordsResponse recordsResponse = new RecordsResponse()
                {
                    TotalRecords = 0,
                    Records = new List<ApplicationUser>()
                };

                DateTime? createDate = null;
                if (!string.IsNullOrEmpty(createDateMiliSecondsString))
                {
                    createDate = (new DateTime(1970, 1, 1)).AddMilliseconds(double.Parse(createDateMiliSecondsString)).Date;
                }

                if (string.IsNullOrEmpty(orderBy) || orderBy == "null")
                {
                    recordsResponse = await _unitOfWork.ApplicationUserRepository.GetAllWithSearchAndFilterAsync(role: role, isActive: isActive, createDate: createDate, search: search, PageSize: PageSize, PageNo: PageNo);
                }
                else if (string.IsNullOrEmpty(order) || order == "null" || order == Order.ASC)
                {
                    /*order ASC*/
                    if (orderBy != ApplicationUserField.CreateDate)
                    {
                        recordsResponse = await _unitOfWork.ApplicationUserRepository.GetAllWithSearchAndFilterAsync(role: role, isActive: isActive, createDate: createDate, search: search, OrderBy: _unitOfWork.ApplicationUserRepository.CreateSelectorExpression(orderBy), Order: Order.ASC, PageSize: PageSize, PageNo: PageNo);
                    }
                    else
                    {
                        recordsResponse = await _unitOfWork.ApplicationUserRepository.GetAllWithSearchAndFilterAsync(role: role, isActive: isActive, createDate: createDate, search: search, OrderBy: u => u.CreateDate.ToString(), Order: Order.ASC, PageSize: PageSize, PageNo: PageNo);
                    }
                }
                else
                {
                    /*order DSE*/
                    if (orderBy != ApplicationUserField.CreateDate)
                    {
                        recordsResponse = await _unitOfWork.ApplicationUserRepository.GetAllWithSearchAndFilterAsync(role: role, isActive: isActive, createDate: createDate, search: search, OrderBy: _unitOfWork.ApplicationUserRepository.CreateSelectorExpression(orderBy), Order: Order.DESC, PageSize: PageSize, PageNo: PageNo);
                    }
                    else
                    {
                        recordsResponse = await _unitOfWork.ApplicationUserRepository.GetAllWithSearchAndFilterAsync(role: role, isActive: isActive, createDate: createDate, search: search, OrderBy: u => u.CreateDate.ToString(), Order: Order.DESC, PageSize: PageSize, PageNo: PageNo);
                    }
                }

                Pagination pagination = new Pagination { PageNo = PageNo, PageSize = PageSize };
                Response.Headers.Append("x-pagination", JsonConvert.SerializeObject(pagination));


                users = recordsResponse.Records;

                IEnumerable<UserResponseDTO> userResponseDTOs = users.Select(user => _mapper.Map<UserResponseDTO>(user));

                _response.Data = new RecordsResponse
                {
                    TotalRecords = recordsResponse.TotalRecords,
                    Records = userResponseDTOs
                };
                _response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                _response.ErrorMessages.Add("Something is wrong.");
                _response.IsSuccess = false;
            }
            return _response;
        }



        [Authorize(Roles = UserRole.Admin)]
        [HttpGet("deactive/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> DeactiveUser(string id)
        {
            try
            {
                if (id == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add("Id not Provided.");
                    return _response;
                }

                var user = await _unitOfWork.ApplicationUserRepository.GetAsync(u => u.Id == id);
                if (user == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages.Add("User not found.");
                    return _response;
                }

                user.IsActive = false;
                user.LockoutEnd = DateTime.UtcNow.AddYears(1000);
                _unitOfWork.ApplicationUserRepository.Update(user);
                _unitOfWork.Save();
                _response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                _response.ErrorMessages.Add("Something is wrong.");
                _response.IsSuccess = false;
            }
            return _response;
        }


        [Authorize(Roles = UserRole.Admin)]
        [HttpGet("active/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> ActiveUser(string id)
        {
            try
            {
                if (id == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add("Id not Provided.");
                    return _response;
                }

                var user = await _unitOfWork.ApplicationUserRepository.GetAsync(u => u.Id == id);
                if (user == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages.Add("User not found.");
                    return _response;
                }

                user.IsActive = true;
                user.LockoutEnd = DateTime.UtcNow.AddDays(-2);
                _unitOfWork.ApplicationUserRepository.Update(user);
                _unitOfWork.Save();
                _response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                _response.ErrorMessages.Add("Something is wrong.");
                _response.IsSuccess = false;
            }
            return _response;
        }


        [Authorize(Roles = UserRole.Admin)]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> Update(UserUpdateDTO userUpdateDTO)
        {
            try
            {
                if (userUpdateDTO == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add("Update Info not Provided.");
                    return _response;
                }



                var userFromDb = await _unitOfWork.ApplicationUserRepository.GetAsync(u => u.Id == userUpdateDTO.Id);
                if (userFromDb == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages.Add("User not found.");
                    return _response;
                }

                var role = await _roleManager.FindByNameAsync(userUpdateDTO.Role);
                if (role == null)
                {
                    userUpdateDTO.Role = UserRole.Salesman;
                }
                userFromDb.Name = userUpdateDTO.Name;
                userFromDb.UpdateDate = userUpdateDTO.UpdateDate;
                if (userFromDb.Role != userUpdateDTO.Role)
                {
                    await _userManager.AddToRoleAsync(userFromDb, userUpdateDTO.Role);
                }

                if (userFromDb.Role != userUpdateDTO.Role)
                {
                    await _userManager.RemoveFromRoleAsync(userFromDb, userFromDb.Role);
                    userFromDb.Role = userUpdateDTO.Role;
                }

                _unitOfWork.ApplicationUserRepository.Update(userFromDb);
                _unitOfWork.Save();
                _response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                _response.ErrorMessages.Add("Something is wrong.");
                _response.IsSuccess = false;
            }
            return _response;
        }


        [Authorize(Roles = UserRole.Admin)]
        [HttpGet("export/excel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ExportExcel()
        {
            var users = await _unitOfWork.ApplicationUserRepository.GetAllAsync();

            DataTable dt = new DataTable();
            dt.TableName = "Users";
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Email", typeof(string));
            dt.Columns.Add("Role", typeof(string));
            dt.Columns.Add("Create Date", typeof(string));
            dt.Columns.Add("Update Date", typeof(string));

            foreach (var user in users)
            {

                dt.Rows.Add(user.Name, user.Email, user.Role, user.CreateDate.ToString("MMMM dd, yyyy"),
                    user.UpdateDate.HasValue ? user.UpdateDate.Value.ToString("MMMM dd, yyyy") : ""
                    );
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.AddWorksheet(dt, "Users");
                using (MemoryStream ms = new MemoryStream())
                {
                    wb.SaveAs(ms);

                    var ExcelFile = File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Users.xlsx");
                    return ExcelFile;
                }
            }

        }
    }
}
