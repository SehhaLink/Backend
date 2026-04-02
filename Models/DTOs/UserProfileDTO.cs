using Sehha360.Models.Enums;

namespace Sehha360.Models.DTOs
{
    public class UserProfileDTO
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateOnly BirthDate { get; set; }
        public int Age {get;set;}
        public Gender Gender { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserRole Role { get; set; }
        public string PhoneNumber { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }
}
