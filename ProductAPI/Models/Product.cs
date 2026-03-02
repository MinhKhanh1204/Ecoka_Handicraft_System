using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductAPI.Models
{
	[Table("PRODUCTS")]
	public class Product
	{
		[Key]
		public string ProductID { get; set; }

		public int CategoryID { get; set; }

		public string ProductName { get; set; } = null!;

		public string? Description { get; set; }

		public string? Material { get; set; }

		public decimal Price { get; set; }

		public decimal Discount { get; set; }

		public int StockQuantity { get; set; }

		public DateTime CreatedAt { get; set; }

		public string Status { get; set; }

		public Category Category { get; set; } = null!;
		public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
	}
}
