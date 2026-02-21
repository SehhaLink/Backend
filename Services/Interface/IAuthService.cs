using Sehha360.Models;
using Sehha360.Models.ApiResponse;
using Sehha360.Models.DTOs;

namespace Sehha360.Services.Interface
{
    public interface IAuthService
    {
        Task<ApiResponse> RegisterAsync(UserRegisterDTO userRegisterDTO);
        Task<ApiResponse> LoginAsync(LoginDTO loginDTO);
        Task<string> GenerateJwtTokenAsync(AppUser appUser);
    }
}
