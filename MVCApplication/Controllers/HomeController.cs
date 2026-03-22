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
            var allProducts = await _productService.GetAllProductsAsync();
            var categories = await _productService.GetAllCategoriesAsync();

            var topDiscounted = allProducts
                .Where(p => p.OriginalPrice > p.FinalPrice)
                .Select(p => new
                {
                    Product = p,
                    DiscountPct = p.OriginalPrice > 0
                        ? (double)((p.OriginalPrice - p.FinalPrice) / p.OriginalPrice * 100)
                        : 0
                })
                .OrderByDescending(x => x.DiscountPct)
                .Take(10)
                .Select(x => x.Product)
                .ToList();

            var vm = new ProductListViewModel
            {
                Products = topDiscounted,
                Categories = categories,
                SectionTitle = "Sản phẩm khuyến mại".ToString()
            };

            return View(vm);
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
