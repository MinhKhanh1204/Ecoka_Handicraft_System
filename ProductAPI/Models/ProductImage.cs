using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductAPI.Models
{
	[Table("PRODUCT_IMAGES")]
	public class ProductImage
	{
		[Key]
		public int ImageID { get; set; }

		public string ProductID { get; set; }

		public string ImageURL { get; set; } = null!;

		public bool IsMain { get; set; }

		public Product Product { get; set; } = null!;
	}
}
