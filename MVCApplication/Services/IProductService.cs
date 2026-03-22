using Microsoft.AspNetCore.Mvc.RazorPages;
using MVCApplication.Models;
using MVCApplication.Models.DTOs;

namespace MVCApplication.Services
{
    public interface IProductService
    {
		Task<List<ProductDto>> GetAllProductsAsync();
        Task<List<ProductDto>> GetTopDiscountProductsAsync(int top = 10);
        Task<CustomFormatter.PagedResult<ProductDto>> GetAllProductsAsync(ProductFilterRequestDto productFilterRequestDto);
		Task<List<CategoryDto>> GetAllCategoriesAsync();
		Task<ProductDetailResponseDto> GetProductDetailAsync(string id);
	}
}
