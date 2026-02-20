using Microsoft.AspNetCore.Mvc;
using ProductAPI.CustomFormatter;
using ProductAPI.DTOs;
using ProductAPI.Services;

namespace ProductAPI.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _service;

        public CategoryController(ICategoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _service.GetAllAsync();
            return Ok(ApiResponse<List<ReadCategoryDto>>
                .SuccessResponse(categories));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _service.GetByIdAsync(id);

            if (category == null)
                return NotFound(ApiResponse<ReadCategoryDto>
                    .Fail("Category not found", 404));

            return Ok(ApiResponse<ReadCategoryDto>
                .SuccessResponse(category));
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CategoryCreateDto dto)
        {
            var category = await _service.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = category.CategoryID },
                ApiResponse<ReadCategoryDto>
                .SuccessResponse(category, "Created successfully"));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] CategoryUpdateDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);

            if (!result)
                return NotFound(ApiResponse<bool>
                    .Fail("Category not found", 404));

            return Ok(ApiResponse<bool>
                .SuccessResponse(true, "Updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);

            if (!result)
                return NotFound(ApiResponse<bool>
                    .Fail("Category not found", 404));

            return Ok(ApiResponse<bool>
                .SuccessResponse(true, "Deleted successfully"));
        }
    }
}
