using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Sehha360.Models;
using Sehha360.Models.ApiResponse;
using Sehha360.Models.DTOs;
using Sehha360.Services.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Sehha360.Services.implementation
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IMapper _mapper;
        public AuthService(UserManager<AppUser> userManager, IMapper mapper, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _mapper = mapper;
            _signInManager = signInManager;
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
            var token = await GenerateJwtTokenAsync(user);
            var userResponse = _mapper.Map<UserResponseDTO>(user);
            userResponse.Token = token;
            return ApiResponse.SuccessResponse("Login Successful", userResponse);
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
