using Microsoft.AspNetCore.Mvc;
using MVCApplication.Areas.Admin.DTOs;
using MVCApplication.Areas.Admin.Services;

namespace MVCApplication.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _service;

        public CategoriesController(ICategoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? keyword, string? status)
        {
            ViewBag.Keyword = keyword;
            ViewBag.Status = status;

            IReadOnlyList<ReadCategoryDto> categories = string.IsNullOrWhiteSpace(keyword)
                ? await _service.GetAllAsync()
                : await _service.SearchAsync(keyword);

            // Lọc theo trạng thái nếu có chọn
            if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "All", StringComparison.OrdinalIgnoreCase))
            {
                categories = categories
                    .Where(c => string.Equals(c.Status, status, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CategoryCreateDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CategoryCreateDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var created = await _service.CreateAsync(dto);
            if (created == null)
            {
                ModelState.AddModelError(string.Empty, "Tạo category thất bại.");
                return View(dto);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _service.GetByIdAsync(id);
            if (category == null) return NotFound();

            var dto = new CategoryUpdateDto
            {
                CategoryName = category.CategoryName,
                Description = category.Description,
                Status = category.Status
            };

            ViewBag.CategoryId = id;
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, CategoryUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CategoryId = id;
                return View(dto);
            }

            var ok = await _service.UpdateAsync(id, dto);
            if (!ok) return NotFound();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
