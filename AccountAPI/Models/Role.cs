using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AccountAPI.Models
{
    [Table("ROLES")]
    public class Role
    {
        [Key]
        public int RoleID { get; set; }

        public string RoleName { get; set; } = null!;
        public string? Description { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
