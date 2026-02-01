using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AccountAPI.Models
{
    [Table("ACCOUNTS")]
    public class Account
    {
        [Key]
        public string AccountID { get; set; }

        [Required]
        public string Username { get; set; } = null!;

        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        public string? PasswordRecoveryToken { get; set; }
        public DateTime? TokenExpiry { get; set; }

        public string? Avatar { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Status { get; set; } = null!;

        // Navigation
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }

}
