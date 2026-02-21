using Sehha360.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Sehha360.Models.DTOs
{
    public class UserRegisterDTO
    {
        [RegularExpression(@"^[a-z A-Z]+$", ErrorMessage = "Full must be letters only.")]
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
        public string FullName { get; set; }
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required(ErrorMessage = "Confirm password is required.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Birth date is required.")]
        public DateOnly BirthDate { get; set; }
        [Required]
        public Gender Gender { get; set; }
        [DataType(DataType.PhoneNumber)]
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public UserRole Role { get; set; }
    }
}
