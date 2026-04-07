using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Sehha360.Models;
using Sehha360.Models.ApiResponse;
using Sehha360.Models.DTOs;
using Sehha360.Repositories.Interface;
using Sehha360.Services.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Sehha360.Services.implementation
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        public AuthService(UserManager<AppUser> userManager, IMapper mapper, SignInManager<AppUser> signInManager, IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _userManager = userManager;
            _mapper = mapper;
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task<ApiResponse> LoginAsync(LoginDTO loginDTO)
        {
            var user = await _userManager.FindByEmailAsync(loginDTO.Email);
            if (user == null)
            {
                return ApiResponse.FaliureResponse("Login Failed", new List<string> { "Invalid email or password" });
            }
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDTO.Password,false);
            if (!result.Succeeded)
            {
                return ApiResponse.FaliureResponse("Login Failed", new List<string> { "Invalid email or password" });
            }

            string message = "Login Successful";
            if (user.IsDeactivated)
            {
                user.IsDeactivated = false;
                user.DeactivationDate = null;
                var updateResult = await _userManager.UpdateAsync(user);
                if (updateResult.Succeeded)
                {
                    message = "Login Successful. Your account has been reactivated.";
                }
            }

            var token = await GenerateJwtTokenAsync(user);
            var userResponse = _mapper.Map<UserResponseDTO>(user);
            userResponse.Token = token;
            return ApiResponse.SuccessResponse(message, userResponse);
        }

        public async Task<ApiResponse> RegisterAsync(UserRegisterDTO userRegisterDTO)
        {
            var user =await _userManager.FindByEmailAsync(userRegisterDTO.Email);
            if (user != null)
            {
                return ApiResponse.FaliureResponse("User Register Failed",new List<string> { "Email already exists" });
            }
            var newUser = _mapper.Map<AppUser>(userRegisterDTO);
            var result  = await _userManager.CreateAsync(newUser, userRegisterDTO.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse.FaliureResponse("User Register Failed", errors);
            }
            var roleresult = await _userManager.AddToRoleAsync(newUser, userRegisterDTO.Role.ToString());
            if(!roleresult.Succeeded)
            {
                var errors = roleresult.Errors.Select(e => e.Description).ToList();
                return ApiResponse.FaliureResponse("User Register Failed", errors);
            }
            return ApiResponse.SuccessResponse("User Registered Successfully");
        }
        public async Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordDTO dto)
        {

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return ApiResponse.SuccessResponse("If an account with that email exists, a password reset code has been sent.");
            }
            var otpCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

            var passwordResetOtp = new OTP
            {
                UserId = user.Id,
                Email = user.Email,
                OtpCode = otpCode,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false
            };

            await _unitOfWork.OTPs.AddAsync(passwordResetOtp);
            await _unitOfWork.SaveChangesAsync();
            var emailSent = await _emailService.SendOtpAsync(user.Email!, otpCode);

            if (!emailSent)
            {
                return ApiResponse.FaliureResponse($"Password reset OTP email failed to send for {user.Email}");
            }

            return ApiResponse.SuccessResponse("If an account with that email exists, a password reset code has been sent.");
        }
        public async Task<ApiResponse> ResetPasswordAsync(ResetPasswordDTO dto)
        {

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return ApiResponse.FaliureResponse("Password reset failed", new List<string> { "Invalid email or OTP code" });
            }
            var otpRecord = await _unitOfWork.OTPs.GetValidOtpAsync(dto.Email, dto.Otp);

            if (otpRecord == null)
            {
                return ApiResponse.FaliureResponse("Password reset failed", new List<string> { "Invalid or expired OTP code" });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse.FaliureResponse("Password reset failed", errors);
            }
            otpRecord.IsUsed = true;
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.SuccessResponse("Password has been reset successfully. You can now log in with your new password.");
        }

        public async Task<ApiResponse> ChangePasswordAsync(ChangePasswordDTO dto, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse.FaliureResponse("User not found");
            }

            var result = await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse.FaliureResponse("Change password failed", errors);
            }

            return ApiResponse.SuccessResponse("Password changed successfully");
        }
        public async Task<string> GenerateJwtTokenAsync(AppUser appUser)
        {
            var roles = await _userManager.GetRolesAsync(appUser);
            var claims = new List<Claim>
            {
               new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
               new Claim(ClaimTypes.NameIdentifier, appUser.Id),
               new Claim(ClaimTypes.Name, appUser.UserName!),
               new Claim(ClaimTypes.Email, appUser.Email!)
            };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            var securityKey = Environment.GetEnvironmentVariable("SecurityKey");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                //issuer: Environment.GetEnvironmentVariable("Issuer"),
                //audience: Environment.GetEnvironmentVariable("Audience"), For testing
                claims: claims,
                expires: DateTime.UtcNow.AddDays(365), // For testing
                signingCredentials: creds
            );
            var loginToken = new JwtSecurityTokenHandler().WriteToken(token);
            return loginToken;
        }
    }
}
