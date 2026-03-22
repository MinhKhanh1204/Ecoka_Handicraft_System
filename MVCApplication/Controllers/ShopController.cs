using Microsoft.AspNetCore.Mvc;
using MVCApplication.CustomFormatter;
using MVCApplication.Models;
using MVCApplication.Models.DTOs;
using MVCApplication.Services;
using System.Net.Http;

namespace MVCApplication.Controllers
{
    public class ShopController : Controller
    {
        private readonly IProductService _productService;

        public ShopController(IProductService productService)
        {
            _productService = productService;
        }

		[HttpGet]
		public async Task<IActionResult> Index(int page = 1, int categoryId = 0, string txtSearch = "")
		{
			var request = new ProductFilterRequestDto
			{
				Page = page,
				CategoryId = categoryId,
				TxtSearch = txtSearch
			};
			var vm = new ProductListViewModel
			{
				TxtSearch = txtSearch,
				CategoryID = categoryId,
				Products = await _productService.GetAllProductsAsync(request),
				Categories = await _productService.GetAllCategoriesAsync()
			};
			CategoryDto all = new CategoryDto()
			{
				CategoryID = 0,
				CategoryName = "All"
			};
			vm.Categories.Insert(0, all);

			return View(vm);
		}

		[HttpGet]
		public async Task<IActionResult> GetProducts(int page = 1, int categoryId = 0, string txtSearch = "")
		{
			var request = new ProductFilterRequestDto
			{
				Page = page,
				CategoryId = categoryId,
				TxtSearch = txtSearch
			};

			var result = await _productService.GetAllProductsAsync(request);

			return PartialView("_ProductListPartial", result);
		}

		[HttpGet]
        public async Task<IActionResult> DetailAsync(string id)
        {
			var product = await _productService.GetProductDetailAsync(id);

			if (product == null)
				return NotFound();

            // Truyền accountID của user đang đăng nhập (null nếu chưa login)
            ViewBag.CurrentUserId = User.FindFirst("accountID")?.Value;

			return View(product);
		}
    }
}
