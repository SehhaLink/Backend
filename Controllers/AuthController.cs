using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Sehha360.Models.ApiResponse;
using Sehha360.Models.DTOs;
using Sehha360.Services.implementation;
using Sehha360.Services.Interface;

namespace Sehha360.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDTO registerDTO)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).
                    Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse.FaliureResponse("Validation failed", errors));
            }
            var response = await _authService.RegisterAsync(registerDTO);
            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).
                    Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse.FaliureResponse("Validation failed", errors));
            }
            var response = await _authService.LoginAsync(loginDTO);
            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO forgotPasswordDTO){
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).
                    Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse.FaliureResponse("Validation failed", errors));
            }
            var response = await _authService.ForgotPasswordAsync(forgotPasswordDTO);
            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO resetPasswordDTO){
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).
                    Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse.FaliureResponse("Validation failed", errors));
            }
            var response = await _authService.ResetPasswordAsync(resetPasswordDTO);
            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO changePasswordDTO)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).
                    Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse.FaliureResponse("Validation failed", errors));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse.FaliureResponse("User is not authorized"));
            }

            var response = await _authService.ChangePasswordAsync(changePasswordDTO, userId);
            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }
    }
}
