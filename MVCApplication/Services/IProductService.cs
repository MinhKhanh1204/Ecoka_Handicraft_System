using MVCApplication.Models.DTOs;

namespace MVCApplication.Services
{
    public interface IProductService
    {
		Task<List<ProductDto>> GetAllProductsAsync();
		Task<List<CategoryDto>> GetAllCategoriesAsync();
		Task<ProductDetailResponseDto> GetProductDetailAsync(string id);
	}
}
