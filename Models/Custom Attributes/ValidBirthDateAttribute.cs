using System.ComponentModel.DataAnnotations;

namespace Sehha360
{
    public sealed class ValidBirthDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null)
            {
                return ValidationResult.Success;
            }

            if (value is not DateOnly birthDate)
            {
                return new ValidationResult("Invalid birth date format");
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (birthDate > today)
            {
                return new ValidationResult("BirthDate cannot be in the future");
            }

            var age = today.Year - birthDate.Year;
            if (birthDate > today.AddYears(-age))
            {
                age--;
            }

            if (age < 0)
            {
                return new ValidationResult("BirthDate is invalid");
            }

            if (age > 120)
            {
                return new ValidationResult("BirthDate is unrealistic");
            }

            return ValidationResult.Success;
        }
    }
}