using System.ComponentModel.DataAnnotations;

namespace MVCApplication.Models
{
    public class RegisterViewModel
    {
        // Account
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự")]
        [MaxLength(100, ErrorMessage = "Mật khẩu không được vượt quá 100 ký tự")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$",
            ErrorMessage = "Mật khẩu phải chứa ít nhất 1 chữ hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt (@$!%*?&)")]
        public string Password { get; set; } = null!;

        public IFormFile? Avatar { get; set; }

        // Customer
        [Required(ErrorMessage = "Họ và tên không được để trống")]
        public string FullName { get; set; } = null!;

        public DateTime? DateOfBirth { get; set; }

        public string? Gender { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }
    }
}