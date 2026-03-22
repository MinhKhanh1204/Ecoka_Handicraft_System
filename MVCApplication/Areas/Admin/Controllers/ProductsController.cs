using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using MVCApplication.Areas.Admin.DTOs;
using MVCApplication.Areas.Admin.Services;
using MVCApplication.Hubs;

namespace MVCApplication.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly IProductAdminService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IHubContext<PendingApprovalHub> _hubContext;
        public ProductsController(IProductAdminService productService, ICategoryService categoryService, IHubContext<PendingApprovalHub> hubContext)
        {
            _productService = productService;
            _categoryService = categoryService;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? keyword, string? status, int pageNumber = 1)
        {
            int pageSize = 10;
            ViewBag.Keyword = keyword;
            ViewBag.Status = status;

            var pagedResult = await _productService.GetPagedAsync(keyword, status, pageNumber, pageSize);

            var isAjax = HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            return isAjax ? PartialView("Index", pagedResult) : View(pagedResult);
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();

            var isAjax = HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            return isAjax ? PartialView("Details", product) : View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = (await _categoryService.GetAllAsync()).Where(c => string.Equals(c.Status, "Active", StringComparison.OrdinalIgnoreCase)).ToList();
            ViewBag.Categories = new SelectList(categories, "CategoryID", "CategoryName");
            var isAjax = HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            return isAjax ? PartialView("Create", new CreateProductDto()) : View(new CreateProductDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductDto dto)
        {
            var isAjax = HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            var categories = (await _categoryService.GetAllAsync()).Where(c => string.Equals(c.Status, "Active", StringComparison.OrdinalIgnoreCase)).ToList();

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(categories, "CategoryID", "CategoryName", dto.CategoryID);
                return isAjax ? PartialView("Create", dto) : View(dto);
            }

            var ok = await _productService.CreateAsync(dto);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, "Failed to create product.");
                ViewBag.Categories = new SelectList(categories, "CategoryID", "CategoryName", dto.CategoryID);
                return isAjax ? PartialView("Create", dto) : View(dto);
            }
            await _hubContext.Clients.All.SendAsync("PendingProductCreated", dto);

            if (isAjax)
            {
                return Json(new { success = true, message = "Product created successfully" });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();

            var allCategories = await _categoryService.GetAllAsync();
            var activeCategories = allCategories.Where(c => string.Equals(c.Status, "Active", StringComparison.OrdinalIgnoreCase)).ToList();

            // Ensure current category is in the list even if inactive, so it can be displayed
            var currentCategory = allCategories.FirstOrDefault(c => c.CategoryName == product.CategoryName);
            if (currentCategory != null && !activeCategories.Any(c => c.CategoryID == currentCategory.CategoryID))
            {
                activeCategories.Add(currentCategory);
            }

            var imageDtos = product.Images.Select((img, idx) => new UpdateProductImageDto
            {
                ImageUrl = img,
                IsMain = (idx == 0)
            }).ToList();

            while (imageDtos.Count < 4)
                imageDtos.Add(new UpdateProductImageDto { ImageUrl = "", IsMain = false });

            var dto = new UpdateProductDto
            {
                CategoryID = currentCategory?.CategoryID ?? 0,
                ProductName = product.ProductName,
                Description = product.Description,
                Material = product.Material,
                Price = product.Price,
                Discount = product.Discount,
                StockQuantity = product.StockQuantity,
                Status = product.Status,
                Images = imageDtos.Take(4).ToList()
            };

            ViewBag.ProductID = id;
            ViewBag.Categories = new SelectList(activeCategories, "CategoryID", "CategoryName", dto.CategoryID);
            var isAjax = HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            return isAjax ? PartialView("Edit", dto) : View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, UpdateProductDto dto)
        {
            var isAjax = HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            var allCategories = await _categoryService.GetAllAsync();
            var activeCategories = allCategories.Where(c => string.Equals(c.Status, "Active", StringComparison.OrdinalIgnoreCase)).ToList();

            if (!ModelState.IsValid)
            {
                // Ensure current category is in the list for validation return
                if (!activeCategories.Any(c => c.CategoryID == dto.CategoryID))
                {
                    var current = allCategories.FirstOrDefault(c => c.CategoryID == dto.CategoryID);
                    if (current != null) activeCategories.Add(current);
                }

                ViewBag.Categories = new SelectList(activeCategories, "CategoryID", "CategoryName", dto.CategoryID);
                ViewBag.ProductID = id;
                return isAjax ? PartialView("Edit", dto) : View(dto);
            }

            try
            {
                var ok = await _productService.UpdateAsync(id, dto);
                if (!ok)
                {
                    ModelState.AddModelError(string.Empty, "Failed to update product.");
                    if (!activeCategories.Any(c => c.CategoryID == dto.CategoryID))
                    {
                        var current = allCategories.FirstOrDefault(c => c.CategoryID == dto.CategoryID);
                        if (current != null) activeCategories.Add(current);
                    }

                    ViewBag.Categories = new SelectList(activeCategories, "CategoryID", "CategoryName", dto.CategoryID);
                    ViewBag.ProductID = id;
                    return isAjax ? PartialView("Edit", dto) : View(dto);
                }
            }
            catch (Exception ex)
            {
                // Surface API error details to the user for debugging and resolution.
                ModelState.AddModelError(string.Empty, ex.Message);

                if (!activeCategories.Any(c => c.CategoryID == dto.CategoryID))
                {
                    var current = allCategories.FirstOrDefault(c => c.CategoryID == dto.CategoryID);
                    if (current != null) activeCategories.Add(current);
                }

                ViewBag.Categories = new SelectList(activeCategories, "CategoryID", "CategoryName", dto.CategoryID);
                ViewBag.ProductID = id;
                return isAjax ? PartialView("Edit", dto) : View(dto);
            }

            if (isAjax)
            {
                return Json(new { success = true, message = "Product updated successfully" });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var isAjax = HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            await _productService.DeleteAsync(id);
            if (isAjax)
            {
                return Json(new { success = true, message = "Product deleted successfully" });
            }
            return RedirectToAction(nameof(Index));
        }
    }
}