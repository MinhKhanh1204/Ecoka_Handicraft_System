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
