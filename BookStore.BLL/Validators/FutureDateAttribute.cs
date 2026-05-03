using System.ComponentModel.DataAnnotations;

namespace ShopNest.BLL.Validators
{
    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            if (value is DateTime date)
            {
                if (date < DateTime.Today)
                    return new ValidationResult("Date must be in the future");
            }

            return ValidationResult.Success;
        }
    }
}