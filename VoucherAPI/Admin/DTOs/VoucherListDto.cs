namespace VoucherAPI.Admin.DTOs
{
    /// <summary>
    /// DTO for voucher list display (UC_46 View vouchers)
    /// </summary>
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
}
