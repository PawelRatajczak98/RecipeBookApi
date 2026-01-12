using Application.DTO;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using FluentValidation.Results;


namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        
        private readonly IValidator<RegisterRequestDto> _registerValidator;
        private readonly IValidator<LoginRequestDto> _loginValidator;
        private readonly IAuthService _authService;
        private readonly IUserContextService _userContextService;

        public AccountController (IValidator<RegisterRequestDto> registerValidator
            , IValidator<LoginRequestDto> loginValidator
            ,IAuthService authService
            ,IUserContextService userContextService)
        {  
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
            _authService = authService;
            _userContextService = userContextService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(RegisterRequestDto registerRequestDto)
        {
            ValidationResult validationResult = await _registerValidator.ValidateAsync(registerRequestDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var result = await _authService.RegisterAsync(registerRequestDto);

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(LoginRequestDto loginRequestDto)
        {

            ValidationResult validationResult = await _loginValidator.ValidateAsync(loginRequestDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }
            string tokenFromLogin = await _authService.LoginAsync(loginRequestDto);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddMinutes(15)
            };
            Response.Cookies.Append("accessToken", tokenFromLogin, cookieOptions);

            return Ok();
        }

        
        [HttpGet("login/me")]
        [Authorize]
        public async Task <IActionResult> GetUserInfo()
        {
            try
            {
                var userDto = await _userContextService.GetUserDto();
                return Ok(userDto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message); 
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message); 
            }
        }
        
    }
}