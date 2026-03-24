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
        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Họ và tên phải từ 3 đến 100 ký tự")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [RegularExpression(@"^(0|\+84)[0-9]{9}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{6,}$",
            ErrorMessage = "Mật khẩu phải chứa chữ hoa, chữ thường, số và ký tự đặc biệt")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        public int RoleID { get; set; }

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Địa chỉ phải từ 5 đến 200 ký tự")]
        public string Address { get; set; } = null!;

        [Required(ErrorMessage = "Giới tính là bắt buộc")] public string Gender { get; set; } = null!;

        [Required(ErrorMessage = "CCCD/CMND là bắt buộc")]
        [RegularExpression(@"^\d{9}(\d{3})?$", ErrorMessage = "CCCD/CMND phải có 9 hoặc 12 chữ số")]
        public string CitizenId { get; set; } = null!;

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        [DataType(DataType.Date)]
        public DateOnly? DateOfBirth { get; set; }

        public IFormFile? AvatarFile { get; set; }
        public string? Avatar { get; set; }
    }

    public class EditStaffViewModel
    {
        [Required(ErrorMessage = "Mã nhân viên là bắt buộc")]
        public string StaffId { get; set; } = null!;

        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Họ và tên phải từ 3 đến 100 ký tự")]
        public string FullName { get; set; } = null!;

        // Không cho chỉnh sửa (readonly trên UI)
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [RegularExpression(@"^(0|\+84)[0-9]{9}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; } = null!;

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Địa chỉ phải từ 5 đến 200 ký tự")]
        public string Address { get; set; } = null!;

        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        public string Gender { get; set; } = null!;

        // Không cho chỉnh sửa (readonly trên UI)
        public string CitizenId { get; set; } = null!;

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        [DataType(DataType.Date)]
        public DateOnly? DateOfBirth { get; set; }

        public bool Status { get; set; }

        public IFormFile? AvatarFile { get; set; }
        public string? Avatar { get; set; }
    }
}