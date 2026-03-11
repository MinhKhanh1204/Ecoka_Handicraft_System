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
        [StringLength(100)]
        public string VoucherName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Code is required")]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, 100, ErrorMessage = "Discount must be between 0.01 and 100")]
        public decimal DiscountPercentage { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MaxReducing { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required]
        [ExpiryDateValidation]
        public DateOnly ExpiryDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MinOrderValue { get; set; }

        [Range(1, int.MaxValue)]
        public int? MaxUsagePerUser { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateVoucherDto
    {
        [Required(ErrorMessage = "Voucher name is required")]
        [StringLength(100)]
        public string VoucherName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, 100, ErrorMessage = "Discount must be between 0.01 and 100")]
        public decimal DiscountPercentage { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MaxReducing { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative")]
        public int Quantity { get; set; }

        [Required]
        [ExpiryDateValidation]
        public DateOnly ExpiryDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MinOrderValue { get; set; }

        [Range(1, int.MaxValue)]
        public int? MaxUsagePerUser { get; set; }

        public bool IsActive { get; set; }
    }
}
