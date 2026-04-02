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
        private readonly IFileStorageService _storageService;

        public UserService(UserManager<AppUser> userManager, IMapper mapper, IFileStorageService storageService)
        {
            _userManager = userManager;
            _mapper = mapper;
            _storageService = storageService;
        }

        public async Task<ApiResponse> GetMeAsync(string userId)
        {
            var appUser = await _userManager.FindByIdAsync(userId);
            if (appUser == null)
            {
                return ApiResponse.FaliureResponse("Unauthorized", new List<string> { "User not found" });
            }

            var dto = _mapper.Map<UserProfileDTO>(appUser);
            if (!string.IsNullOrEmpty(appUser.ProfilePictureKey))
            {
                dto.ProfilePictureUrl = await _storageService.GetPreSignedUrlAsync(appUser.ProfilePictureKey, TimeSpan.FromHours(1));
            }
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
            if (!string.IsNullOrEmpty(appUser.ProfilePictureKey))
            {
                responseDto.ProfilePictureUrl = await _storageService.GetPreSignedUrlAsync(appUser.ProfilePictureKey, TimeSpan.FromHours(1));
            }
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

        public async Task<ApiResponse> UploadProfilePictureAsync(string userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return ApiResponse.FaliureResponse("No file uploaded");

            if (file.Length > 5 * 1024 * 1024)
                return ApiResponse.FaliureResponse("File size exceeds 5 MB limit");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return ApiResponse.FaliureResponse("Invalid file type. Only JPG, JPEG, and PNG are allowed.");

            var appUser = await _userManager.FindByIdAsync(userId);
            if (appUser == null)
            {
                return ApiResponse.FaliureResponse("User not found");
            }

            try
            {
                // Delete old profile picture if exists
                if (!string.IsNullOrEmpty(appUser.ProfilePictureKey))
                {
                    await _storageService.DeleteFileAsync(appUser.ProfilePictureKey);
                }

                using var stream = file.OpenReadStream();
                var key = await _storageService.UploadFileAsync(stream, $"profile_{userId}{extension}", file.ContentType);

                appUser.ProfilePictureKey = key;
                var result = await _userManager.UpdateAsync(appUser);

                if (!result.Succeeded)
                {
                    await _storageService.DeleteFileAsync(key); // Rollback storage
                    return ApiResponse.FaliureResponse("Failed to update user profile with new image key.");
                }

                var signedUrl = await _storageService.GetPreSignedUrlAsync(key, TimeSpan.FromHours(1));
                return ApiResponse.SuccessResponse("Profile picture updated successfully", new { imageUrl = signedUrl });
            }
            catch (Exception ex)
            {
                return ApiResponse.FaliureResponse($"Error uploading profile picture: {ex.Message}");
            }
        }
    }
}
