using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sehha360.Models.ApiResponse;
using Sehha360.Models.DTOs;
using Sehha360.Services.Interface;
using System.Security.Claims;

namespace Sehha360.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized(ApiResponse.FaliureResponse("Unauthorized", new List<string> { "Restricted Access" }));
            }

            var response = await _userService.GetMeAsync(userId);
            if (response.Success)
            {
                return Ok(response);
            }

            return Unauthorized(response);
        }

        [HttpPatch("me")]
        public async Task<IActionResult> UpdateMe([FromBody] UpdateMeDTO dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse.FaliureResponse("Validation failed", errors));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized(ApiResponse.FaliureResponse("Unauthorized", new List<string> { "Missing user id claim" }));
            }

            var response = await _userService.UpdateMeAsync(userId, dto);
            if (response.Success)
            {
                return Ok(response);
            }

            return Unauthorized(response);
        }
    }
}
