namespace Sehha360.Services.Interface
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task<bool> SendOtpAsync(string to, string otp);
    }
}
