using MVCApplication.CustomFormatter;
using MVCApplication.Models.DTOs;

namespace MVCApplication.Services.Implements
{
	public class ProductService : IProductService
	{
		private readonly HttpClient _httpClient;

		public ProductService(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}
		public async Task<List<ProductDto>> GetAllProductsAsync()
		{
			var response = await _httpClient.GetAsync("/products");
			response.EnsureSuccessStatusCode();

			var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<ProductDto>>>();
			return result?.Data ?? new List<ProductDto>();
		}

		public async Task<List<CategoryDto>> GetAllCategoriesAsync()
		{
			var response = await _httpClient.GetAsync("/categories");
			response.EnsureSuccessStatusCode();

			var result = await response.Content
				  .ReadFromJsonAsync<ApiResponse<List<CategoryDto>>>();

			return result?.Data ?? new List<CategoryDto>();
		}

		public async Task<ProductDetailResponseDto> GetProductDetailAsync(string id)
		{
			var response = await _httpClient.GetAsync($"/products/{id}");
			response.EnsureSuccessStatusCode();

			var result = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDetailResponseDto>>();

			return result?.Data;
		}
	}
}
