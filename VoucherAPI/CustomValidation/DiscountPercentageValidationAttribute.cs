using System.ComponentModel.DataAnnotations;

namespace VoucherAPI.CustomValidation
{
    /// <summary>
    /// Custom validation for DiscountPercentage - must be greater than 0
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class DiscountPercentageValidationAttribute : ValidationAttribute
    {
        public DiscountPercentageValidationAttribute()
        {
            ErrorMessage = "Discount percentage must be greater than 0";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            decimal discount;
            if (value is decimal decimalValue)
            {
                discount = decimalValue;
            }
            else if (value is double doubleValue)
            {
                discount = (decimal)doubleValue;
            }
            else
            {
                return new ValidationResult("Invalid discount percentage value");
            }

            if (discount <= 0)
            {
                return new ValidationResult(ErrorMessage);
            }

            if (discount > 100)
            {
                return new ValidationResult("Discount percentage cannot exceed 100");
            }

            return ValidationResult.Success;
        }
    }
}
