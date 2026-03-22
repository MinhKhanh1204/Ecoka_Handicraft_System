namespace MVCApplication.Areas.Admin.DTOs
{
    /// <summary>
    /// Result object for voucher write operations (Create/Update).
    /// Carries success/failure status AND the server error message for display.
    /// </summary>
    public class VoucherOperationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int? VoucherId { get; set; }
    }
}
