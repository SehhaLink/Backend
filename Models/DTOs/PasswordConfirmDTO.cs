using System.ComponentModel.DataAnnotations;

namespace Sehha360.Models.DTOs
{
    public class PasswordConfirmDTO
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
