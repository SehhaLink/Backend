using Microsoft.EntityFrameworkCore;
using Sehha360.Data;
using Sehha360.Models;
using Sehha360.Repositories.Interface;

namespace Sehha360.Repositories.Implementation
{
    public class OtpRepository: Repository<OTP>, IOtpRepository
    {
        public OtpRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<OTP?> GetValidOtpAsync(string email, string otpCode)
        {
            return await _dbSet
                .Where(o => o.Email.ToLower() == email.ToLower() &&
                           o.OtpCode == otpCode &&
                           !o.IsUsed &&
                           o.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }
}
