using System.ComponentModel.DataAnnotations;

namespace AccountAPI.DTOs
{
    public class RegisterCustomerRequestDto
    {
        // Account
        public string Username { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Password { get; set; } = null!;

        public IFormFile? Avatar { get; set; }

        // Customer
        public string FullName { get; set; } = null!;

        public DateTime? DateOfBirth { get; set; }

        public string? Gender { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }
    }
}
