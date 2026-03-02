using System.ComponentModel.DataAnnotations;

namespace MVCApplication.Models
{
    // Generic API Response from backend
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public T? Data { get; set; }
    }

    // Customer View Model for Admin
    public class CustomerViewModel
    {
        public string CustomerID { get; set; } = null!;
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Avatar { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    // Customer Update Model with Validation
    public class CustomerUpdateModel
    {
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string? FullName { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [RegularExpression("^(Male|Female|Other)$", ErrorMessage = "Gender must be Male, Female, or Other")]
        [Display(Name = "Gender")]
        public string? Gender { get; set; }

        [RegularExpression(@"^(0[1-9][0-9]{8}|0[1-9][0-9]{9})$", ErrorMessage = "Phone number must be 9-10 digits starting with 0")]
        [StringLength(10, ErrorMessage = "Phone cannot exceed 10 characters")]
        [Display(Name = "Phone")]
        public string? Phone { get; set; }

        [StringLength(255, ErrorMessage = "Address cannot exceed 255 characters")]
        [Display(Name = "Address")]
        public string? Address { get; set; }
    }

    // Customer Status Update Model with Validation
    public class CustomerStatusModel
    {
        [Required(ErrorMessage = "Status is required")]
        [RegularExpression("^(Active|Inactive)$", ErrorMessage = "Status must be either 'Active' or 'Inactive'")]
        [Display(Name = "Status")]
        public string Status { get; set; } = null!;
    }

    // Customer Search Model with Validation
    public class CustomerSearchModel
    {
        [StringLength(100, ErrorMessage = "Keyword cannot exceed 100 characters")]
        [Display(Name = "Keyword")]
        public string? Keyword { get; set; }

        [RegularExpression("^(Active|Inactive|)$", ErrorMessage = "Status must be either 'Active' or 'Inactive'")]
        [Display(Name = "Status")]
        public string? Status { get; set; }
    }
}
