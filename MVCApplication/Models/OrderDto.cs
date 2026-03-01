using System.ComponentModel.DataAnnotations;

namespace MVCApplication.Models
{
    public class Order
    {
        [Required]
        public string OrderID { get; set; } = null!;
        public string? CustomerID { get; set; }
        public string? StaffID { get; set; }
        public int? VoucherID { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public string? ShippingStatus { get; set; }
        public string? ShippingAddress { get; set; }
        public string? Note { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<OrderItem>? OrderItems { get; set; }
    }

    public class OrderCreateDto
    {
        [Required]
        public string CustomerID { get; set; } = null!;

        public string? StaffID { get; set; }
        public int? VoucherID { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = null!;

        [Required]
        public string ShippingAddress { get; set; } = null!;

        public string? Note { get; set; }

        public List<OrderItemCreateDto> OrderItems { get; set; } = new();
    }

    public class OrderUpdateDto
    {
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public string? ShippingStatus { get; set; }
        public string? ShippingAddress { get; set; }
        public string? Note { get; set; }
    }

}
