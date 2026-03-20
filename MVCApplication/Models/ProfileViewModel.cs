using System.ComponentModel.DataAnnotations;

namespace MVCApplication.Models
{
    public class ProfileViewModel
    {
        // Account
        public string AccountId { get; set; } = null!;

        public string Username { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? Avatar { get; set; }

        // Customer
        [Required(ErrorMessage = "FullName is required")]
        public string FullName { get; set; } = null!;

        public DateTime? DateOfBirth { get; set; }

        public string? Gender { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

    }
}
