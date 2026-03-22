using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MVCApplication.Areas.Admin.DTOs;
using MVCApplication.Areas.Admin.Services;
using MVCApplication.Hubs;

namespace MVCApplication.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _service;
        private readonly IHubContext<CategoryHub> _hubContext;

        public CategoriesController(
            ICategoryService service,
            IHubContext<CategoryHub> hubContext)
        {
            _service = service;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? keyword, string? status)
        {
            ViewBag.Keyword = keyword;
            ViewBag.Status = status;

            IReadOnlyList<ReadCategoryDto> categories = string.IsNullOrWhiteSpace(keyword)
                ? await _service.GetAllAsync()
                : await _service.SearchAsync(keyword);

            if (!string.IsNullOrWhiteSpace(status) &&
                !string.Equals(status, "All", StringComparison.OrdinalIgnoreCase))
            {
                categories = categories
                    .Where(c => string.Equals(c.Status, status, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CategoryList", categories);
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

            // nếu cần thì đảm bảo mặc định pending ở đây hoặc trong service
            // dto.Status = "Pending";

            var created = await _service.CreateAsync(dto);
            if (created == null)
            {
                ModelState.AddModelError(string.Empty, "Tạo category thất bại.");
                return View(dto);
            }

            await _hubContext.Clients.All.SendAsync("PendingCategoryCreated", dto);

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
            var result = await _service.DeleteAsync(id);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = result, message = result ? "Xóa thành công" : "Xóa thất bại" });
            }

            return RedirectToAction(nameof(Index));
        }
    }
}