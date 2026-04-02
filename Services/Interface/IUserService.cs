using Sehha360.Models.ApiResponse;
using Sehha360.Models.DTOs;

namespace Sehha360.Services.Interface
{
    public interface IUserService
    {
        Task<ApiResponse> GetMeAsync(string userId);
        Task<ApiResponse> UpdateMeAsync(string userId, UpdateMeDTO dto);
        Task<ApiResponse> DeactivateMeAsync(string userId);
        Task<ApiResponse> HardDeleteMeAsync(string userId);
        Task<ApiResponse> UploadProfilePictureAsync(string userId, IFormFile file);
    }
}
