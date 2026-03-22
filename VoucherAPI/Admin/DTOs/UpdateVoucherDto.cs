using System.ComponentModel.DataAnnotations;
using VoucherAPI.CustomValidation;

namespace VoucherAPI.Admin.DTOs
{
    /// <summary>
    /// DTO for editing voucher (UC_49 Edit voucher)
    /// </summary>
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
