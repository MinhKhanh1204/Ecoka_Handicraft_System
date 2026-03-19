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
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Full name must be 3-100 characters")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Phone is required")]
        [RegularExpression(@"^(0|\+84)[0-9]{9}$", ErrorMessage = "Invalid phone number")]
        public string Phone { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{6,}$",
            ErrorMessage = "Password must contain uppercase, lowercase, number and special character")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Role is required")]
        public int RoleID { get; set; }

        // ✅ Address (bắt buộc + độ dài)
        [Required(ErrorMessage = "Address is required")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Address must be 5-200 characters")]
        public string Address { get; set; } = null!;

        // ✅ Gender (bắt buộc)
        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; } = null!;

        // ✅ CCCD
        [Required(ErrorMessage = "Citizen ID is required")]
        [RegularExpression(@"^\d{9}(\d{3})?$", ErrorMessage = "Citizen ID must be 9 or 12 digits")]
        public string CitizenId { get; set; } = null!;

        // ✅ DOB
        [Required(ErrorMessage = "Date of birth is required")]
        [DataType(DataType.Date)]
        public DateOnly? DateOfBirth { get; set; }

        // 🖼 Avatar (optional)
        public IFormFile? AvatarFile { get; set; }
        public string? Avatar { get; set; }
    }

    public class EditStaffViewModel
    {
        [Required(ErrorMessage = "Staff ID is required")]
        public string StaffId { get; set; } = null!;

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Full name must be between 3 and 100 characters")]
        public string FullName { get; set; } = null!;

        // Not editable (readonly in UI) -> no validation rules here
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Phone is required")]
        [RegularExpression(@"^(0|\+84)[0-9]{9}$", ErrorMessage = "Invalid phone number")]
        public string Phone { get; set; } = null!;

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Address must be 5-200 characters")]
        public string Address { get; set; } = null!;

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; } = null!;

        // Not editable (readonly in UI) -> no validation rules here
        public string CitizenId { get; set; } = null!;

        [Required(ErrorMessage = "Date of birth is required")]
        [DataType(DataType.Date)]
        public DateOnly? DateOfBirth { get; set; }

        public bool Status { get; set; }

        public IFormFile? AvatarFile { get; set; }
        public string? Avatar { get; set; }
    }
}
