using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ShopNest.BLL.Validators
{
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxSizeInMB;

        public MaxFileSizeAttribute(int maxSizeInMB)
        {
            _maxSizeInMB = maxSizeInMB;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            if (value is IFormFile file)
            {
                if (file.Length > _maxSizeInMB * 1024 * 1024)
                    return new ValidationResult($"File size exceeds {_maxSizeInMB}MB");
            }

            return ValidationResult.Success;
        }
    }
}