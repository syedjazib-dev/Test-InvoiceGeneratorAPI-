using CRM.Models;
using InvoiceGenerator.Model.Models.DTOs;
using InvoiceGenerator.StaticData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using InvoiceGenrator.Model.Models;
using InvoiceGenerator.DataAccess.Repository.IRepositoy;
using System.Net.Http;

namespace InvoiceGenratorAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private APIResponse _response;
       
        private readonly IUnitOfWork _unitOfWork;

        public AuthController( IUnitOfWork unitOfWork)
        {
            _response = new APIResponse();
            _unitOfWork = unitOfWork;
        }


        [Authorize(Roles = UserRole.Admin)]
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> Register([FromBody] UserCreateRequestDTO userCreateRequestDTO)
        {
            try
            {
                if (userCreateRequestDTO == null) { 
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add("User creation info not provided.");
                    return _response;
                }

                bool isUniqueEmail = await _unitOfWork.ApplicationUserRepository.IsUniqueUser(userCreateRequestDTO.Email);
                if (!isUniqueEmail)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Email Already Registered");
                }

                await _unitOfWork.ApplicationUserRepository.Register(userCreateRequestDTO);
                _response.StatusCode = HttpStatusCode.OK; 
            }
            catch (Exception e)
            {
                _response.ErrorMessages.Add("Something is wrong.");
                _response.IsSuccess = false;
            }
            return _response;
        }



        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> Login([FromBody] UserLoginRequest userLoginRequest)
        {
            try
            {
                if (userLoginRequest == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add("User credentials not provided.");
                    return _response;
                }

                UserLoginResponseDTO userLoginResposeDTO = await _unitOfWork.ApplicationUserRepository.Login(userLoginRequest);



                CookieOptions cookieOptions = new CookieOptions()
                {
                    Expires = DateTime.UtcNow.AddDays(30),
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.None
                };

                HttpContext.Response.Cookies.Append(CookieKey.AccessToken, userLoginResposeDTO.TokenDTO.AccessToken, cookieOptions);
                HttpContext.Response.Cookies.Append(CookieKey.RefreshToken, userLoginResposeDTO.TokenDTO.RefreshToken, cookieOptions);


                _response.Data = userLoginResposeDTO.UserId;
                _response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                _response.ErrorMessages.Add(e.Message);
                _response.IsSuccess = false;
            }
            return _response;
        }

        [HttpPost("mobile/login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> MobileLogin([FromBody] UserLoginRequest userLoginRequest)
        {
            try
            {
                if (userLoginRequest == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add("User credentials not provided.");
                    return _response;
                }

                UserLoginResponseDTO userLoginResposeDTO = await _unitOfWork.ApplicationUserRepository.LoginMobile(userLoginRequest);


                _response.Data = userLoginResposeDTO;
                _response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                _response.ErrorMessages.Add(e.Message);
                _response.IsSuccess = false;
            }
            return _response;
        }

        [HttpGet("refreshToken")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> RefreshToken()
        {
            try
            {
                var accessToken =  HttpContext.Request.Cookies[CookieKey.AccessToken];
                if (accessToken == null) { 
                    _response.IsSuccess=false;
                    _response.ErrorMessages.Add("Access token not existes.");
                    _response.StatusCode=HttpStatusCode.BadRequest;
                    return _response;
                }

                var refreshToken = HttpContext.Request.Cookies[CookieKey.RefreshToken];
                if (refreshToken == null) {
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Refresh token not existes.");
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return _response;
                }


                TokenDTO tokenDTO = await _unitOfWork.ApplicationUserRepository.RefreshToken(refreshToken);



                CookieOptions cookieOptions = new CookieOptions()
                {
                    Expires = DateTime.UtcNow.AddDays(30),
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.None
                };

                HttpContext.Response.Cookies.Append(CookieKey.AccessToken, tokenDTO.AccessToken, cookieOptions);
                HttpContext.Response.Cookies.Append(CookieKey.RefreshToken, tokenDTO.RefreshToken, cookieOptions);

                _response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                _response.ErrorMessages.Add("Something is wrong.");
                _response.IsSuccess = false;
            }
            return _response;
        }


        [Authorize]
        [HttpGet("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> logout()
        {
            try
            {
                CookieOptions cookieOptions = new CookieOptions()
                {
                    Expires = DateTime.UtcNow.AddDays(-1),
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.None
                };

                if (HttpContext.Request.Cookies[CookieKey.RefreshToken] != null)
                {
                    var refreshToken = HttpContext.Request.Cookies[CookieKey.RefreshToken];
                    var refreshTokenFromDb = await _unitOfWork.RefreshTokenRepository.GetAsync(u => u.Token == refreshToken);
                    await _unitOfWork.RefreshTokenRepository.RemoveAsync(refreshTokenFromDb);
                    _unitOfWork.Save();
                    HttpContext.Response.Cookies.Append(CookieKey.RefreshToken, "", cookieOptions);
                }
                if (HttpContext.Request.Cookies[CookieKey.AccessToken] != null)
                {
                    HttpContext.Response.Cookies.Append(CookieKey.AccessToken, "", cookieOptions);
                }


                _response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                _response.ErrorMessages.Add("Something is wrong.");
                _response.IsSuccess = false;
            }
            return _response;
        }

    }
}
