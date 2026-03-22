using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ProductAPI.CustomFormatter;
using ProductAPI.DTOs;
using ProductAPI.Models;
using ProductAPI.Services;

namespace ProductAPI.Controllers
{
	[ApiController]
	[Route("api/products")]
	public class ProductController : ControllerBase
	{
		private readonly IProductService _service;

		public ProductController(IProductService service)
		{
			_service = service;
		}

		[HttpGet]
		public IActionResult GetAll()
		{
            var products = _service.GetAllProducts();
            return Ok(ApiResponse<List<ProductDto>>.SuccessResponse(products));
        }

		[HttpPost("filter")]
		public async Task<IActionResult> FilterProducts(ProductFilterRequestDto request)
		{
			var result = await _service.FilterProductsAsync(request);
			return Ok(ApiResponse<PagedResult<ProductDto>>.SuccessResponse(result));
		}

		[HttpGet("{id}")]
        public async Task<IActionResult> GetProductDetail(string id)
        {
            var result = await _service.GetProductDetailAsync(id);
            return Ok(ApiResponse<ProductDetailResponseDto>.SuccessResponse(result));
        }

        [HttpPut("{id}/stock")]
        public async Task<IActionResult> UpdateStock(string id, [FromBody] int quantityChange)
        {
            var success = await _service.UpdateStockAsync(id, quantityChange);
            if (!success) return NotFound(ApiResponse<bool>.Fail("Product not found or update failed", 404));
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Stock updated successfully"));
        }

                [HttpGet("top-discount")]
        public async Task<IActionResult> GetTopDiscountProducts()
        {
            var result = await _service.GetTopDiscountProductsAsync(10);
            return Ok(ApiResponse<List<ProductDto>>.SuccessResponse(result));
        }
    }
}
