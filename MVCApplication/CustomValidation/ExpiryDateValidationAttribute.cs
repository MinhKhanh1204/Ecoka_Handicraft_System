using System.ComponentModel.DataAnnotations;

namespace MVCApplication.CustomValidation
{
    /// <summary>
    /// Custom validation for ExpiryDate - must be today or in the future
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ExpiryDateValidationAttribute : ValidationAttribute
    {
        public ExpiryDateValidationAttribute()
        {
            ErrorMessage = "Expiry date must be today or in the future";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (value is DateOnly expiryDate)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                if (expiryDate < today)
                {
                    return new ValidationResult(ErrorMessage);
                }
            }
            else if (value is DateTime expiryDateTime)
            {
                var today = DateTime.Now.Date;
                if (expiryDateTime.Date < today)
                {
                    return new ValidationResult(ErrorMessage);
                }
            }

            return ValidationResult.Success;
        }
    }
}
