using System.ComponentModel.DataAnnotations.Schema;

namespace AccountAPI.Models
{
    [Table("USER_ROLE")]
    public class UserRole
    {
        public string AccountID { get; set; }
        public int RoleID { get; set; }

        public string Status { get; set; } = null!;

        // Navigation
        public Account Account { get; set; } = null!;
        public Role Role { get; set; } = null!;
    }

}
