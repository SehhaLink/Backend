using System.ComponentModel.DataAnnotations;

namespace Sehha360.Models.DTOs
{
    public class ForgotPasswordDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
