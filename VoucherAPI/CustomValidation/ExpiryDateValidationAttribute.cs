using System.ComponentModel.DataAnnotations;

namespace VoucherAPI.CustomValidation
{
    /// <summary>
    /// Custom validation for ExpiryDate - must be today or in the future
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ExpiryDateValidationAttribute : ValidationAttribute
    {
        public ExpiryDateValidationAttribute() : base(() => "Expiry date must be today or in the future") { }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            DateTime today = DateTime.Now.Date;

            if (value is DateOnly expiryDate)
            {
                DateOnly todayOnly = DateOnly.FromDateTime(DateTime.Now);
                if (expiryDate < todayOnly)
                {
                    return new ValidationResult(ErrorMessage);
                }
            }
            else if (value is DateTime expiryDateTime)
            {
                if (expiryDateTime.Date < today)
                {
                    return new ValidationResult(ErrorMessage);
                }
            }
            else
            {
                return new ValidationResult("Invalid expiry date format");
            }

            return ValidationResult.Success;
        }
    }
}
