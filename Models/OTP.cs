using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sehha360.Models
{
    public class OTP
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string OtpCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
    }
}
