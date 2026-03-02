using ProductAPI.DTOs;

namespace ProductAPI.Services
{
	public interface IProductService
	{
		List<ProductDto> GetAllProducts();
        Task<ProductDetailResponseDto> GetProductDetailAsync(string productId);
    }
}
