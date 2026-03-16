namespace PaymentAPI.DTOs
{
    public class PaymentRequestDto
    {
        public string OrderId { get; set; } = null!;
        public decimal Amount { get; set; }
        public string OrderInfo { get; set; } = "";
        public string ReturnUrl { get; set; } = "";
    }

    public class PaymentResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string PaymentUrl { get; set; } = "";
    }
}
