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
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{6,}$",
                ErrorMessage = "Mật khẩu phải chứa chữ hoa, chữ thường, số và ký tự đặc biệt")]
            public string Password { get; set; } = null!;

            [Required(ErrorMessage = "Vai trò là bắt buộc")]
            public int RoleID { get; set; }

            [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
            public string Address { get; set; } = null!;

            [Required(ErrorMessage = "Giới tính là bắt buộc")]
            public string Gender { get; set; } = null!;
            [Required(ErrorMessage = "CCCD/CMND là bắt buộc")]
            [RegularExpression(@"^\d{9}(\d{3})?$", ErrorMessage = "CCCD/CMND không hợp lệ")]
            public string CitizenId { get; set; } = null!;

            [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
            public DateOnly? DateOfBirth { get; set; }

            public string? Avatar { get; set; }
        }

        public class UpdateStaffDto
        {
            [Required(ErrorMessage = "Mã nhân viên là bắt buộc")]
            public string StaffId { get; set; } = null!;

            [Required(ErrorMessage = "Họ và tên là bắt buộc")]
            [StringLength(100, MinimumLength = 3, ErrorMessage = "Họ và tên phải từ 3 đến 100 ký tự")]
            public string FullName { get; set; } = null!;

            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            public string Email { get; set; } = null!;

            [RegularExpression(@"^(0|\+84)[0-9]{9}$", ErrorMessage = "Số điện thoại không hợp lệ")]
            public string Phone { get; set; } = null!;

            [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
            [StringLength(200, ErrorMessage = "Địa chỉ tối đa 200 ký tự")]
            public string Address { get; set; } = null!;

            [Required(ErrorMessage = "Giới tính là bắt buộc")]
            public string Gender { get; set; } = null!;

            [RegularExpression(@"^\d{9}(\d{3})?$", ErrorMessage = "CCCD/CMND không hợp lệ")]
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