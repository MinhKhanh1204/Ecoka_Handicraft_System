using System.ComponentModel.DataAnnotations;

namespace AccountAPI.DTOs
{
    public class ChangePasswordDto
    {
        [Required]
        public string OldPassword { get; set; } = null!;

        [Required]
        [MinLength(6, ErrorMessage = "New password must be at least 6 characters long")]
        public string NewPassword { get; set; } = null!;

        [Required]
        [Compare("NewPassword", ErrorMessage = "Confirm password does not match")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
