using System.ComponentModel.DataAnnotations;

namespace MVCApplication.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = null!;
    }
}
