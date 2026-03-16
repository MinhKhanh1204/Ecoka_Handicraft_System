namespace OrderAPI.DTOs
{
    public class PaymentStatusUpdateDto
    {
        public string PaymentMethod { get; set; } = "Online";
        public string Status { get; set; } = null!;
        public string? Note { get; set; }
    }
}
