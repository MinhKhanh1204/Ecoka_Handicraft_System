using Microsoft.AspNetCore.Mvc.RazorPages;
using MVCApplication.Models.DTOs;

namespace MVCApplication.Models
{
	public class ProductListViewModel
	{
		public string TxtSearch { get; set; } = "";
		public int CategoryID { get; set; } = 0;
		public CustomFormatter.PagedResult<ProductDto> Products { get; set; }
		public List<CategoryDto> Categories { get; set; }
        public string SectionTitle { get; internal set; }
    }
}
