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
            [Required(ErrorMessage = "Full name is required")]
            [StringLength(100, MinimumLength = 3)]
            public string FullName { get; set; } = null!;

            [Required]
            [EmailAddress(ErrorMessage = "Invalid email format")]
            public string Email { get; set; } = null!;

            [Required]
            [RegularExpression(@"^(0|\+84)[0-9]{9}$", ErrorMessage = "Invalid phone number")]
            public string Phone { get; set; } = null!;

            [Required]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{6,}$",
                ErrorMessage = "Password must contain uppercase, lowercase, number and special character")]
            public string Password { get; set; } = null!;

            [Required(ErrorMessage = "Role is required")]
            public int RoleID { get; set; }

            [Required(ErrorMessage = "Address is required")]
            public string Address { get; set; } = null!;

            [Required(ErrorMessage = "Gender is required")]
            public string Gender { get; set; } = null!;

            [Required(ErrorMessage = "Citizen ID is required")]
            [RegularExpression(@"^\d{9}(\d{3})?$", ErrorMessage = "Invalid Citizen ID")]
            public string CitizenId { get; set; } = null!;

            [Required(ErrorMessage = "Date of birth is required")]
            public DateOnly? DateOfBirth { get; set; }

            public string? Avatar { get; set; }
        }

        public class UpdateStaffDto
        {
            [Required(ErrorMessage = "StaffId is required")]
            public string StaffId { get; set; } = null!;

            [Required(ErrorMessage = "Full name is required")]
            [StringLength(100, MinimumLength = 3, ErrorMessage = "Full name must be between 3 and 100 characters")]
            public string FullName { get; set; } = null!;

            [EmailAddress(ErrorMessage = "Invalid email address")]
            public string Email { get; set; } = null!;

            [RegularExpression(@"^(0|\+84)[0-9]{9}$", ErrorMessage = "Invalid phone number")]
            public string Phone { get; set; } = null!;

            [Required(ErrorMessage = "Address is required")]
            [StringLength(200, ErrorMessage = "Address max length is 200 characters")]
            public string Address { get; set; } = null!;

            [Required(ErrorMessage = "Gender is required")]
            public string Gender { get; set; } = null!;

            [RegularExpression(@"^\d{9}(\d{3})?$", ErrorMessage = "Invalid Citizen ID")]
            public string CitizenId { get; set; } = null!;

            public DateOnly? DateOfBirth { get; set; }

            public bool Status { get; set; }

            public string? Avatar { get; set; }
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
