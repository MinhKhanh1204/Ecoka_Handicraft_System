using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountAPI.Models
{
    [Table("STAFFS")]
    public class Staff
    {
        [Key]
        public string StaffId { get; set; } = null!;

        public string? FullName { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        public string? Gender { get; set; }

        public string? Phone { get; set; }

        public string? CitizenId { get; set; }

        public string? Address { get; set; }

        public DateOnly? HireDate { get; set; }

        public string? Status { get; set; }

        public virtual Account StaffNavigation { get; set; } = null!;
    }
}