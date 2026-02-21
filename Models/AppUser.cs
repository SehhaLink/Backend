using Microsoft.AspNetCore.Identity;
using Sehha360.Models.Enums;

namespace Sehha360.Models
{
    public class AppUser: IdentityUser
    {
        public string FullName { get; set; }
        public DateOnly BirthDate { get; set; }
        public Gender Gender { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
