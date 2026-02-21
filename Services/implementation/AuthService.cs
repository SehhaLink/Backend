using Microsoft.AspNetCore.Identity;
using Sehha360.Models;
using Sehha360.Models.ApiResponse;
using Sehha360.Models.DTOs;
using Sehha360.Services.Interface;

namespace Sehha360.Services.implementation
{
    public class AuthService : IAuthService
    {
        public async Task<ApiResponse> LoginAsync(LoginDTO loginDTO)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse> RegisterAsync(UserRegisterDTO userRegisterDTO)
        {
            throw new NotImplementedException();
        }
        public async Task<string> GenerateJwtTokenAsync(AppUser appUser)
        {
            throw new NotImplementedException();
        }
    }
}
