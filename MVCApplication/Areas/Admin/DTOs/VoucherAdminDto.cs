using System.ComponentModel.DataAnnotations;
using MVCApplication.CustomValidation;

namespace MVCApplication.Areas.Admin.DTOs
{
    public class VoucherListDto
    {
        public int VoucherId { get; set; }
        public string? VoucherName { get; set; }
        public string? Code { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal? MaxReducing { get; set; }
        public int? Quantity { get; set; }
        public DateOnly? ExpiryDate { get; set; }
        public bool? IsActive { get; set; }
    }

    public class VoucherDetailDto
    {
        public int VoucherId { get; set; }
        public string? VoucherName { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal? MaxReducing { get; set; }
        public int? Quantity { get; set; }
        public int? UsageCount { get; set; }
        public DateOnly? ExpiryDate { get; set; }
        public bool? IsActive { get; set; }
        public decimal? MinOrderValue { get; set; }
        public int? MaxUsagePerUser { get; set; }
    }

    public class CreateVoucherDto
    {
        [Required(ErrorMessage = "Voucher name is required")]
        [StringLength(100, ErrorMessage = "Voucher name cannot exceed 100 characters")]
        public string VoucherName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Code is required")]
        [StringLength(50, ErrorMessage = "Code cannot exceed 50 characters")]
        public string Code { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Discount percentage is required")]
        [Range(0.01, 50, ErrorMessage = "Discount percentage must be between 0.01% and 50%")]
        public decimal DiscountPercentage { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Max reducing amount cannot be negative")]
        public decimal? MaxReducing { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Expiry date is required")]
        [ExpiryDateValidation(ErrorMessage = "Expiry date must be today or in the future")]
        public DateOnly ExpiryDate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Minimum order value cannot be negative")]
        public decimal? MinOrderValue { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Max usage per user must be at least 1")]
        public int? MaxUsagePerUser { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateVoucherDto
    {
        [Required(ErrorMessage = "Voucher name is required")]
        [StringLength(100, ErrorMessage = "Voucher name cannot exceed 100 characters")]
        public string VoucherName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Discount percentage is required")]
        [Range(0.01, 50, ErrorMessage = "Discount percentage must be between 0.01% and 50%")]
        public decimal DiscountPercentage { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Max reducing amount cannot be negative")]
        public decimal? MaxReducing { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Expiry date is required")]
        [ExpiryDateValidation(ErrorMessage = "Expiry date must be today or in the future")]
        public DateOnly ExpiryDate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Minimum order value cannot be negative")]
        public decimal? MinOrderValue { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Max usage per user must be at least 1")]
        public int? MaxUsagePerUser { get; set; }

        public bool IsActive { get; set; }
    }
}
