using Microsoft.AspNetCore.Mvc;
using MVCApplication.Models;
using MVCApplication.Services;

namespace MVCApplication.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private readonly ICategoryAdminService _service;

        public CategoriesController(ICategoryAdminService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _service.GetAllAsync();
            return Json(categories);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return await GetAll();

            var categories = await _service.SearchAsync(keyword);
            return Json(categories);
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var category = await _service.GetByIdAsync(id);
            if (category == null)
                return NotFound();
            return Json(category);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.CreateAsync(dto);
            if (result == null)
                return BadRequest(new { message = "Failed to create category." });

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _service.UpdateAsync(id, dto);
            if (!success)
                return BadRequest(new { message = "Failed to update category." });

            return Ok(new { message = "Updated successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
                return BadRequest(new { message = "Failed to delete category." });

            return Ok(new { message = "Deleted successfully." });
        }
    }
}
