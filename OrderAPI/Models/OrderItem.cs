using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderAPI.Models
{
    [Table("ORDER_ITEMS")]
    public class OrderItem
    {
        [Key]
        public int OrderItemID { get; set; }

        public string? OrderID { get; set; }
        public string? ProductID { get; set; }
        public int? Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? UnitPrice { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? Discount { get; set; }

        // Navigation
        [ForeignKey("OrderID")]
        public Order? Order { get; set; }
    }
}