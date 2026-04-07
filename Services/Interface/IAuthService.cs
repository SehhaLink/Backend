using Sehha360.Models;
using Sehha360.Models.ApiResponse;
using Sehha360.Models.DTOs;

namespace Sehha360.Services.Interface
{
    public interface IAuthService
    {
        Task<ApiResponse> RegisterAsync(UserRegisterDTO userRegisterDTO);
        Task<ApiResponse> LoginAsync(LoginDTO loginDTO);
        Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordDTO forgotPasswordDto);
        Task<ApiResponse> ResetPasswordAsync(ResetPasswordDTO resetPasswordDto);
        Task<ApiResponse> ChangePasswordAsync(ChangePasswordDTO changePasswordDto, string userId);
        Task<string> GenerateJwtTokenAsync(AppUser appUser);
    }
}
