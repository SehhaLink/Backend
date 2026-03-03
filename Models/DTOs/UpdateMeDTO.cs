using Sehha360.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Sehha360.Models.DTOs
{
    public class UpdateMeDTO
    {
        [MinLength(2)]
        public string? FullName { get; set; }
        [ValidBirthDate]
        public DateOnly? BirthDate { get; set; }
        public Gender? Gender { get; set; }
        [Phone]
        public string? PhoneNumber { get; set; }
    }
}
