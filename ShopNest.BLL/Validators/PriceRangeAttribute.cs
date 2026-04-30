using System.ComponentModel.DataAnnotations;

namespace ShopNest.BLL.Validators
{
    public class PriceRangeAttribute : ValidationAttribute
    {
        private readonly double _min;
        private readonly double _max;

        public PriceRangeAttribute(double min, double max)
        {
            _min = min;
            _max = max;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            if (value is decimal price)
            {
                if (price < (decimal)_min || price > (decimal)_max)
                    return new ValidationResult($"Price must be between {_min} and {_max}");
            }

            return ValidationResult.Success;
        }
    }
}