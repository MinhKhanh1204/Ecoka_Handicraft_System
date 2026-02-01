using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductAPI.Models
{
	[Table("CATEGORIES")]
	public class Category
	{
		[Key]
		public int CategoryID { get; set; }

		public string CategoryName { get; set; } = null!;

		public string? Description { get; set; }

		public string Status { get; set; }

		public ICollection<Product> Products { get; set; } = new List<Product>();
	}
}
