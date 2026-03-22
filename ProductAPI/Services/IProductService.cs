using ProductAPI.CustomFormatter;
using ProductAPI.DTOs;

namespace ProductAPI.Services
{
	public interface IProductService
	{
		List<ProductDto> GetAllProducts();
		Task<PagedResult<ProductDto>> FilterProductsAsync(ProductFilterRequestDto request);
		Task<ProductDetailResponseDto> GetProductDetailAsync(string productId);
        Task<List<ProductDto>> GetTopDiscountProductsAsync(int top = 10);
    }
}
