using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCApplication.Models;
using MVCApplication.Services;
using System.Diagnostics;

namespace MVCApplication.Controllers
{
	public class HomeController : Controller
	{
        private readonly IProductService _productService;

        public HomeController(IProductService productService)
        {
            _productService = productService;
        }

		public async Task<IActionResult> Index()
		{
            var top10Discount = await _productService.GetTopDiscountProductsAsync();
            return View(top10Discount);
		}
	}
}
