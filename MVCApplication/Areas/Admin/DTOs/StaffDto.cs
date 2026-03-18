using System.ComponentModel.DataAnnotations;

namespace MVCApplication.Areas.Admin.DTOs
{
    // Staff list item (from API ReadStaffDto)
    public class StaffViewModel
    {
        public string StaffId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string? Avatar { get; set; }
        public bool Status { get; set; }
    }

    // Staff detail (from API StaffDetailDto)
    public class StaffDetailViewModel
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
        public string? HireDate { get; set; }
    }

    // Paged result from API
    public class StaffPagedResult
    {
        public List<StaffViewModel> Items { get; set; } = new();
        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    // Create staff form
    public class CreateStaffViewModel
    {
        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Phone is required")]
        [RegularExpression(@"^(0[1-9][0-9]{8,9})$", ErrorMessage = "Phone must be 10-11 digits starting with 0")]
        [Display(Name = "Phone")]
        public string Phone { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = null!;

        [Display(Name = "Role")]
        [Required]
        public int RoleID { get; set; } = 2;

        [StringLength(255)]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        [Display(Name = "Gender")]
        public string? Gender { get; set; }

        [Display(Name = "Citizen ID")]
        [RegularExpression(@"^[0-9]{9,12}$", ErrorMessage = "Citizen ID must be 9-12 digits")]
        public string? CitizenId { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateOnly? DateOfBirth { get; set; }
    }

    // Edit staff form — StaffId and CitizenId are NOT editable
    public class EditStaffViewModel
    {
        [Required]
        public string StaffId { get; set; } = null!;

        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = null!;

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Display(Name = "Phone")]
        [RegularExpression(@"^(0[1-9][0-9]{8,9})$", ErrorMessage = "Phone must be 10-11 digits starting with 0")]
        public string Phone { get; set; } = null!;

        [StringLength(255)]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        [Display(Name = "Gender")]
        public string? Gender { get; set; }

        // Read-only in edit — displayed but not submitted
        [Display(Name = "Citizen ID")]
        public string? CitizenId { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateOnly? DateOfBirth { get; set; }

        [Display(Name = "Status")]
        public bool Status { get; set; }
    }
}
