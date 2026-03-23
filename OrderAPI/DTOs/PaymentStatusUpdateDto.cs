namespace OrderAPI.DTOs
{
    public class PaymentStatusUpdateDto
    {
        // UI sends "PaymentMethod" — add this property so model-binding works.
        public string? PaymentMethod { get; set; }

        // kept for backward compatibility with existing callers
        public string NewPaymentStatus { get; set; } = "Online";

        // maps to repository/service "paymentStatus"
        public string Status { get; set; } = null!;

        public string? Note { get; set; }

        // optional: admin/staff id when admin updates payment status
        public string? StaffId { get; set; }
    }
}
