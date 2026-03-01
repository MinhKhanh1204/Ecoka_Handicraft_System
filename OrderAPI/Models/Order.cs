using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderAPI.Models
{
    [Table("ORDERS")]
    public class Order
    {
        [Key]
        public string OrderID { get; set; } = null!;

        public string? CustomerID { get; set; }
        public string? StaffID { get; set; }
        public int? VoucherID { get; set; }

        public DateTime? OrderDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalAmount { get; set; }

        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public string? ShippingStatus { get; set; }
        public string? ShippingAddress { get; set; }
        public string? Note { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    public enum PaymentStatus
    {
        Pending,
        Paid,
        Refunded,
        Failed
    }

    public enum ShippingStatus
    {
        Pending,
        Approved,
        Cancelled,
        Returned
    }
}