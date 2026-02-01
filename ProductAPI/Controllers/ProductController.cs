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
	}
}
