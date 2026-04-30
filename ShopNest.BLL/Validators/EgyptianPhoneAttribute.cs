using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ShopNest.BLL.Validators
{
    public class EgyptianPhoneAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            if (value is string phone && !string.IsNullOrEmpty(phone))
            {
                var regex = new Regex(@"^01[0125][0-9]{8}$");
                if (!regex.IsMatch(phone))
                    return new ValidationResult("Invalid Egyptian phone number");
            }

            return ValidationResult.Success;
        }
    }
}