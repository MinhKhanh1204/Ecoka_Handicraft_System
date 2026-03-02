using Microsoft.AspNetCore.Mvc;
using ProductAPI.Admin.DTOs;
using ProductAPI.Admin.Services;

namespace ProductAPI.Admin.Controllers
{
    [ApiController]
    [Route("api/admin/categories")]
    [Tags("AdminCategoryAPI")]
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
            return Ok(categories);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest("Keyword is required");

            var categories = await _service.SearchAsync(keyword);
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _service.GetByIdAsync(id);

            if (category == null)
                return NotFound("Category not found");

            return Ok(category);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CategoryCreateDto dto)
        {
            var category = await _service.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = category.CategoryID },
                category);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] CategoryUpdateDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);

            if (!result)
                return NotFound("Category not found");

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);

            if (!result)
                return NotFound("Category not found");

            // Soft delete đã được thực hiện trong Repository (Status = "Deleted")
            return NoContent();
        }
    }
}
