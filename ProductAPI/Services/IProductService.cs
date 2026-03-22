using ProductAPI.CustomFormatter;
using ProductAPI.DTOs;

namespace ProductAPI.Services
{
	public interface IProductService
	{
		List<ProductDto> GetAllProducts();
        Task<ProductDetailResponseDto> GetProductDetailAsync(string productId);
        Task<bool> UpdateStockAsync(string productId, int quantityChange);
		Task<PagedResult<ProductDto>> FilterProductsAsync(ProductFilterRequestDto request);
        Task<List<ProductDto>> GetTopDiscountProductsAsync(int top = 10);
    }
}
