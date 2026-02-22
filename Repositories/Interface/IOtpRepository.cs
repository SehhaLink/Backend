using Sehha360.Models;

namespace Sehha360.Repositories.Interface
{
    public interface IOtpRepository: IRepository<OTP>
    {
        Task<OTP?> GetValidOtpAsync(string email, string otpCode);
    }
}
