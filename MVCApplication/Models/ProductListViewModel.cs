using MVCApplication.Models.DTOs;

namespace MVCApplication.Models
{
	public class ProductListViewModel
	{
		public List<ProductDto> Products { get; set; }
		public List<CategoryDto> Categories { get; set; }
	}
}
