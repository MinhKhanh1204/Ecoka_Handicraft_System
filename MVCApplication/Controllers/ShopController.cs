using Microsoft.AspNetCore.Mvc;
using MVCApplication.Models;
using MVCApplication.Services;

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
        public async Task<IActionResult> IndexAsync()
        {
			var vm = new ProductListViewModel
			{
				Products = await _productService.GetAllProductsAsync(),
				Categories = await _productService.GetAllCategoriesAsync()
			};

			return View(vm);
		}

        [HttpGet]
        public async Task<IActionResult> DetailAsync(string id)
        {
			var product = await _productService.GetProductDetailAsync(id);

			if (product == null)
				return NotFound();

			return View(product);
		}
    }
}
