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
        private readonly IHubContext<PendingApprovalHub> _pendingHub;
        private readonly IHubContext<CategoryHub> _categoryHub;

        public CategoriesController(
            ICategoryService service,
            IHubContext<PendingApprovalHub> pendingHub,
            IHubContext<CategoryHub> categoryHub)
        {
            _service = service;
            _pendingHub = pendingHub;
            _categoryHub = categoryHub;
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
        [ValidateAntiForgeryToken]
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

            await _pendingHub.Clients.All.SendAsync("PendingCategoryCreated", new
            {
                categoryId = created.CategoryID,
                categoryName = created.CategoryName,
                description = created.Description,
                status = created.Status
            });

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CategoryId = id;
                return View(dto);
            }

            var ok = await _service.UpdateAsync(id, dto);

            if (!ok) return NotFound();
            await _categoryHub.Clients.All.SendAsync("CategoryUpdated", new
            {
                categoryId = id
            });
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = result,
                    message = result ? "Xóa thành công" : "Xóa thất bại"
                });
            }

            return RedirectToAction(nameof(Index));
        }
    }
}