using System.ComponentModel.DataAnnotations;

namespace AccountAPI.Admin.DTOs
{
    public class CustomerDto
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

    public class CustomerUpdateDto
    {
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        public string? FullName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [RegularExpression("^(Male|Female|Other)$", ErrorMessage = "Gender must be Male, Female, or Other")]
        public string? Gender { get; set; }

        [RegularExpression(@"^(0[1-9][0-9]{8}|0[1-9][0-9]{9})$", ErrorMessage = "Phone number must be 9-10 digits starting with 0")]
        [StringLength(10, ErrorMessage = "Phone cannot exceed 10 characters")]
        public string? Phone { get; set; }

        [StringLength(255, ErrorMessage = "Address cannot exceed 255 characters")]
        public string? Address { get; set; }
    }

    public class CustomerStatusDto
    {
        [Required(ErrorMessage = "Status is required")]
        [RegularExpression("^(Active|Inactive)$", ErrorMessage = "Status must be either 'Active' or 'Inactive'")]
        public string Status { get; set; } = null!;
    }

    public class CustomerSearchDto
    {
        [StringLength(100, ErrorMessage = "Keyword cannot exceed 100 characters")]
        public string? Keyword { get; set; }

        [RegularExpression("^(Active|Inactive|)$", ErrorMessage = "Status must be either 'Active' or 'Inactive'")]
        public string? Status { get; set; }
    }
}
