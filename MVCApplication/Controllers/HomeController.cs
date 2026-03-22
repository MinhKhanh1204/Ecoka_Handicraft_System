using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MVCApplication.Models;
using MVCApplication.Services;

namespace MVCApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductService _productService;
        private readonly IFeedbackService _feedbackService;
        private readonly ICustomerService _customerService;
        public HomeController(ILogger<HomeController> logger, IProductService productService, IFeedbackService feedbackService, ICustomerService customerService)
        {
            _logger = logger;
            _productService = productService;
            _feedbackService = feedbackService;
            _customerService = customerService;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

            if (User.IsInRole("Employee"))
                return RedirectToAction("Index", "Dashboard", new { area = "Employee" });

            var topDiscounted = await _productService.GetTopDiscountProductsAsync();
            return View(topDiscounted);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public async Task<IActionResult> GetHomepageFeedbacks(int? minRating = 4, int take = 10)
        {
            var feedbacks = await _feedbackService.FilterAsync(new FeedbackFilterDto
            {
                Status = "Active",
                MinRating = minRating
            });

            var allProducts = await _productService.GetAllProductsAsync();
            var productDict = allProducts.ToDictionary(p => p.ProductID, p => p);

            var result = new List<HomepageFeedbackDto>();

            foreach (var x in feedbacks
                .OrderByDescending(f => f.CreatedAt)
                .Take(take)
                .ToList())
            {
                productDict.TryGetValue(x.ProductID ?? "", out var product);

                string customerName = x.Username ?? "Khách hàng";

                if (!string.IsNullOrWhiteSpace(x.CustomerID))
                {
                    var account = await _customerService.GetByIdAsync(x.CustomerID);
                    if (account != null && !string.IsNullOrWhiteSpace(account.FullName))
                    {
                        customerName = account.FullName;
                    }
                }

                result.Add(new HomepageFeedbackDto
                {
                    FeedbackID = x.FeedbackID,
                    CustomerID = x.CustomerID,
                    CustomerName = customerName,
                    ProductID = x.ProductID,
                    ProductName = product?.ProductName ?? "Sản phẩm",
                    ProductImage = !string.IsNullOrWhiteSpace(product?.MainImage)
                        ? product.MainImage
                        : "/img/fruite-item-1.jpg",
                    Rating = x.Rating,
                    Comment = string.IsNullOrWhiteSpace(x.Comment)
                        ? "Khách hàng chưa để lại nội dung."
                        : x.Comment,
                    CreatedAt = x.CreatedAt
                });
            }

            return Json(result);
        }
    }
}
