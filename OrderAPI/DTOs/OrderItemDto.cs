using System.ComponentModel.DataAnnotations;

namespace OrderAPI.DTOs
{
    public class OrderItemReadDto
    {
        public int OrderItemID { get; set; }
        public string? ProductID { get; set; }
        public int? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? Discount { get; set; }
    }

    public class OrderItemCreateDto
    {
        [Required]
        public string ProductID { get; set; } = null!;

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Range(0, 100)]
        public decimal Discount { get; set; }
    }

    // Make update properties nullable so AutoMapper's "skip nulls" condition works as expected.
    public class OrderItemUpdateDto
    {
        public int? Quantity { get; set; }

        public decimal? UnitPrice { get; set; }

        public decimal? Discount { get; set; }
    }
}
