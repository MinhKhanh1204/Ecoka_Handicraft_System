using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MVCApplication.Areas.Admin.DTOs;
using MVCApplication.Areas.Admin.Services;

namespace MVCApplication.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly IProductAdminService _productService;
        private readonly ICategoryService _categoryService;

        public ProductsController(IProductAdminService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? keyword, string? status, int pageNumber = 1)
        {
            int pageSize = 10;
            ViewBag.Keyword = keyword;
            ViewBag.Status = status;

            var pagedResult = await _productService.GetPagedAsync(keyword, status, pageNumber, pageSize);

            return View(pagedResult);
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();

            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = (await _categoryService.GetAllAsync()).Where(c => string.Equals(c.Status, "Active", StringComparison.OrdinalIgnoreCase)).ToList();
            ViewBag.Categories = new SelectList(categories, "CategoryID", "CategoryName");
            return View(new CreateProductDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductDto dto)
        {
            var categories = (await _categoryService.GetAllAsync()).Where(c => string.Equals(c.Status, "Active", StringComparison.OrdinalIgnoreCase)).ToList();

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(categories, "CategoryID", "CategoryName", dto.CategoryID);
                return View(dto);
            }

            var ok = await _productService.CreateAsync(dto);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, "Failed to create product.");
                ViewBag.Categories = new SelectList(categories, "CategoryID", "CategoryName", dto.CategoryID);
                return View(dto);
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
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, UpdateProductDto dto)
        {
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
                return View(dto);
            }

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
                return View(dto);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            await _productService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}