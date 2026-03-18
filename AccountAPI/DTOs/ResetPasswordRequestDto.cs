using System.ComponentModel.DataAnnotations;

namespace AccountAPI.DTOs
{
    public class ResetPasswordRequestDto
    {
        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; } = null!;

        [Required(ErrorMessage = "New password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [MaxLength(100, ErrorMessage = "Password must not exceed 100 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character (@$!%*?&)")]
        public string NewPassword { get; set; } = null!;
    }
}
