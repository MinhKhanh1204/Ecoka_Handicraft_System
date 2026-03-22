<<<<<<< HEAD
﻿using ProductAPI.CustomFormatter;
=======
using ProductAPI.CustomFormatter;
>>>>>>> bcf93a6f09ad5705e4c5c79e6444c15a4ebf0c29
using ProductAPI.DTOs;

namespace ProductAPI.Services
{
	public interface IProductService
	{
		List<ProductDto> GetAllProducts();		
		Task<PagedResult<ProductDto>> FilterProductsAsync(ProductFilterRequestDto request);
		Task<ProductDetailResponseDto> GetProductDetailAsync(string productId);
        Task<List<ProductDto>> GetTopDiscountProductsAsync(int top = 10);
        Task<bool> UpdateStockAsync(string productId, int quantityChange);
    }
}
