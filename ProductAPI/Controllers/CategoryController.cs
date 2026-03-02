using Microsoft.AspNetCore.Mvc;
using ProductAPI.CustomFormatter;
using ProductAPI.DTOs;
using ProductAPI.Models;
using ProductAPI.Services;

namespace ProductAPI.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _service;

        public CategoryController(ICategoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var categories = _service.GetAllCategories();
            return Ok(ApiResponse<List<CategoryDto>>.SuccessResponse(categories));
        }
    }
}
