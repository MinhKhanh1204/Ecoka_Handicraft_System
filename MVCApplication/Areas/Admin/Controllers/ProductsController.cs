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
            var categories = await _categoryService.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "CategoryID", "CategoryName");
            return View(new CreateProductDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductDto dto)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _categoryService.GetAllAsync();
                ViewBag.Categories = new SelectList(categories, "CategoryID", "CategoryName", dto.CategoryID);
                return View(dto);
            }

            var ok = await _productService.CreateAsync(dto);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, "Failed to create product.");
                var categories = await _categoryService.GetAllAsync();
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

            var categories = await _categoryService.GetAllAsync();
            var category = categories.FirstOrDefault(c => c.CategoryName == product.CategoryName);

            var imageDtos = product.Images.Select((img, idx) => new UpdateProductImageDto
            {
                ImageUrl = img,
                IsMain = (idx == 0)
            }).ToList();

            while (imageDtos.Count < 4)
                imageDtos.Add(new UpdateProductImageDto { ImageUrl = "", IsMain = false });

            var dto = new UpdateProductDto
            {
                CategoryID = category?.CategoryID ?? 0,
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
            ViewBag.Categories = new SelectList(categories, "CategoryID", "CategoryName", dto.CategoryID);
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, UpdateProductDto dto)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _categoryService.GetAllAsync();
                ViewBag.Categories = new SelectList(categories, "CategoryID", "CategoryName", dto.CategoryID);
                ViewBag.ProductID = id;
                return View(dto);
            }

            var ok = await _productService.UpdateAsync(id, dto);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, "Failed to update product.");
                var categories = await _categoryService.GetAllAsync();
                ViewBag.Categories = new SelectList(categories, "CategoryID", "CategoryName", dto.CategoryID);
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