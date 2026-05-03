using System.ComponentModel.DataAnnotations;

namespace ShopNest.BLL.Validators
{
    public class YearRangeAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            if (value is int year)
            {
                if (year < 1900 || year > DateTime.Now.Year)
                    return new ValidationResult($"Year must be between 1900 and {DateTime.Now.Year}");
            }

            return ValidationResult.Success;
        }
    }
}