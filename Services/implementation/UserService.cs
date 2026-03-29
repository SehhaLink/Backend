using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Sehha360.Models;
using Sehha360.Models.ApiResponse;
using Sehha360.Models.DTOs;
using Sehha360.Services.Interface;

namespace Sehha360.Services.implementation
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public UserService(UserManager<AppUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<ApiResponse> GetMeAsync(string userId)
        {
            var appUser = await _userManager.FindByIdAsync(userId);
            if (appUser == null)
            {
                return ApiResponse.FaliureResponse("Unauthorized", new List<string> { "User not found" });
            }

            var dto = _mapper.Map<UserProfileDTO>(appUser);
            return ApiResponse.SuccessResponse("User profile retrieved successfully", dto);
        }

        public async Task<ApiResponse> UpdateMeAsync(string userId, UpdateMeDTO dto)
        {
            var appUser = await _userManager.FindByIdAsync(userId);
            if (appUser == null)
            {
                return ApiResponse.FaliureResponse("Unauthorized", new List<string> { "User not found" });
            }

            if (!string.IsNullOrWhiteSpace(dto.FullName))
            {
                appUser.FullName = dto.FullName.Trim();
            }

            if (dto.BirthDate.HasValue)
            {
                appUser.BirthDate = dto.BirthDate.Value;
            }

            if (dto.Gender.HasValue)
            {
                appUser.Gender = dto.Gender.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
            {
                appUser.PhoneNumber = dto.PhoneNumber.Trim();
            }

            var result = await _userManager.UpdateAsync(appUser);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse.FaliureResponse("Profile update failed", errors);
            }

            var responseDto = _mapper.Map<UserProfileDTO>(appUser);
            return ApiResponse.SuccessResponse("Profile updated successfully", responseDto);
        }
        public async Task<ApiResponse> DeactivateMeAsync(string userId)
        {
            var appUser = await _userManager.FindByIdAsync(userId);
            if (appUser == null)
            {
                return ApiResponse.FaliureResponse("User not found");
            }

            if (appUser.IsDeactivated)
            {
                return ApiResponse.FaliureResponse("Account is already deactivated");
            }

            appUser.IsDeactivated = true;
            appUser.DeactivationDate = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(appUser);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse.FaliureResponse("Deactivation failed", errors);
            }

            return ApiResponse.SuccessResponse("Account deactivated successfully. You have 30 days to reactivate your account by logging in.");
        }

        public async Task<ApiResponse> HardDeleteMeAsync(string userId)
        {
            var appUser = await _userManager.FindByIdAsync(userId);
            if (appUser == null)
            {
                return ApiResponse.FaliureResponse("User not found");
            }

            var result = await _userManager.DeleteAsync(appUser);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse.FaliureResponse("Account deletion failed", errors);
            }

            return ApiResponse.SuccessResponse("Account and all associated data have been permanently deleted.");
        }
    }
}
