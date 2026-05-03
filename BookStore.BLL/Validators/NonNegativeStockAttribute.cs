using System.ComponentModel.DataAnnotations;

namespace ShopNest.BLL.Validators
{
    public class NonNegativeStockAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            if (value is int stock)
            {
                if (stock < 0)
                    return new ValidationResult("Stock cannot be negative");
            }

            return ValidationResult.Success;
        }
    }
}