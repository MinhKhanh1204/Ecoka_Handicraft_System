using System.ComponentModel.DataAnnotations;

namespace MVCApplication.Models
{
    public class RegisterViewModel
    {
        // Account
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [MaxLength(100, ErrorMessage = "Password must not exceed 100 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$",
                    ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character (@$!%*?&)")]
        public string Password { get; set; } = null!;

        public IFormFile? Avatar { get; set; }

        // Customer
        [Required(ErrorMessage = "FullName is required")]
        public string FullName { get; set; } = null!;

        public DateTime? DateOfBirth { get; set; }

        public string? Gender { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }
    }
}
