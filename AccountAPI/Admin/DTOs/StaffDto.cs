using System.ComponentModel.DataAnnotations;

namespace AccountAPI.Admin.DTOs
{
    public class StaffDto
    {
        public class ReadStaffDto
        {
            public string StaffId { get; set; } = null!;

            public string FullName { get; set; } = null!;

            public string Email { get; set; } = null!;

            public string Phone { get; set; } = null!;

            public string Role { get; set; } = null!;

            public string? Avatar { get; set; }

            public bool Status { get; set; }
        }

        public class PagedResult<T>
        {
            public IEnumerable<T> Items { get; set; } = new List<T>();

            public int TotalItems { get; set; }

            public int Page { get; set; }

            public int PageSize { get; set; }
        }

        public class StaffDetailDto
        {
            public string StaffId { get; set; } = null!;

            public string FullName { get; set; } = null!;

            public string Email { get; set; } = null!;

            public string Phone { get; set; } = null!;

            public string Address { get; set; } = null!;

            public string Role { get; set; } = null!;

            public string? Avatar { get; set; }

            public string? Gender { get; set; }

            public string? CitizenId { get; set; }

            public DateOnly? DateOfBirth { get; set; }

            public bool Status { get; set; }

            public DateOnly? HireDate { get; set; }
        }

        public class CreateStaffDto
        {
            [Required]
            public string FullName { get; set; } = null!;

            [Required]
            [EmailAddress]
            public string Email { get; set; } = null!;

            [Required]
            public string Phone { get; set; } = null!;

            [Required]
            public string Password { get; set; } = null!;

            public int RoleID { get; set; }

            public string? Address { get; set; }

            public string? Gender { get; set; }

            public string? CitizenId { get; set; }

            public DateOnly? DateOfBirth { get; set; }
        }

        public class UpdateStaffDto
        {
            [Required]
            public string StaffId { get; set; } = null!;

            [Required]
            public string FullName { get; set; } = null!;

            [EmailAddress]
            public string Email { get; set; } = null!;

            public string Phone { get; set; } = null!;

            public string? Address { get; set; }

            public string? Gender { get; set; }

            public DateOnly? DateOfBirth { get; set; }

            public bool Status { get; set; }
        }

        public class StaffSearchDto
        {
            public string? Keyword { get; set; }

            public string? Role { get; set; }

            public bool? Status { get; set; }

            public int Page { get; set; } = 1;

            public int PageSize { get; set; } = 10;
        }
    }
}
