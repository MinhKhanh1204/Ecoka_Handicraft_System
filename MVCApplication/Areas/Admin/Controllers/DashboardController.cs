using Microsoft.AspNetCore.Mvc;
using MVCApplication.Areas.Admin.Services;
using MVCApplication.Services;
using MVCApplication.Models.DTOs;
using MVCApplication.Areas.Admin.DTOs;

namespace MVCApplication.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly IAdminOrderService _adminOrderService;
        private readonly IProductAdminService _productService;
        private readonly IFeedbackService _feedbackService;
        private readonly IVoucherService _voucherService;
        private readonly ICategoryService _categoryService;

        public DashboardController(
            IAdminOrderService adminOrderService,
            IProductAdminService productService,
            IFeedbackService feedbackService,
            IVoucherService voucherService,
            ICategoryService categoryService)
        {
            _adminOrderService = adminOrderService;
            _productService = productService;
            _feedbackService = feedbackService;
            _voucherService = voucherService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(int? revenueYear)
        {
            var year = revenueYear ?? DateTime.Now.Year;

            // ===============================
            // Run services in parallel
            // ===============================

            var revenueTask = _adminOrderService.GetRevenueByYearAsync(year);
            var ordersTask = _adminOrderService.GetAllOrdersForStaffAsync();
            var feedbackTask = _feedbackService.GetAllAsync();
            var productsPendingTask = _productService.GetPagedAsync(null, "Pending", "Admin", 1, 1);
            var vouchersTask = _voucherService.GetAllVouchersAsync();

            await Task.WhenAll(
                revenueTask,
                ordersTask,
                feedbackTask,
                productsPendingTask,
                vouchersTask
            );

            var revenueData = revenueTask.Result ?? new List<RevenueByMonthDto>();

            var orders = ordersTask.Result ?? Enumerable.Empty<Order>();

            var feedbacks = feedbackTask.Result ?? Enumerable.Empty<dynamic>();

            var productsPending = productsPendingTask.Result;

            var vouchers = vouchersTask.Result ?? Enumerable.Empty<VoucherDto>();


            // ===============================
            // UC_71 - Orders statistics
            // ===============================

            int totalOrders = orders.Count();

            long totalItemsSold = orders
                .SelectMany(o => o.OrderItems ?? Enumerable.Empty<OrderItem>())
                .Sum(i => i.Quantity ?? 0);


            // ===============================
            // Orders by Month
            // ===============================

            var ordersByMonth = orders
                .Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Year == year)
                .GroupBy(o => o.OrderDate.Value.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToList();


            // ===============================
            // UC_72 - Feedback statistics
            // ===============================

            int positiveFeedback = 0;
            int negativeFeedback = 0;

            foreach (var f in feedbacks)
            {
                try
                {
                    var ratingProp = f.GetType().GetProperty("Rating");

                    if (ratingProp == null) continue;

                    var val = ratingProp.GetValue(f);

                    if (val == null) continue;

                    int rating = Convert.ToInt32(val);

                    if (rating >= 4)
                        positiveFeedback++;
                    else if (rating <= 2)
                        negativeFeedback++;
                }
                catch { }
            }


            // ===============================
            // UC_74 - Pending Products
            // ===============================

            int productPendingCount = productsPending?.TotalCount ?? 0;


            // ===============================
            // UC_75 - Pending Vouchers
            // ===============================

            int voucherApprovalCount =
                vouchers.Count(v => !(v.IsActive ?? false));


            // ===============================
            // UC_76 - Categories Approval
            // ===============================

            int categoryApprovalCount = 0;

            try
            {
                var categories = await _categoryService.GetAllAsync();

                if (categories != null)
                {
                    categoryApprovalCount =
                        categories.Count(c => c.Status == "Pending");
                }
            }
            catch
            {
                categoryApprovalCount = 0;
            }


            // ===============================
            // Build Dashboard Stats
            // ===============================

            var stats = new DashboardStats(
                RevenueData: revenueData,
                OrdersByMonth: ordersByMonth,
                SelectedRevenueYear: year,
                TotalOrders: totalOrders,
                TotalItemsSold: totalItemsSold,
                PositiveFeedbackCount: positiveFeedback,
                NegativeFeedbackCount: negativeFeedback,
                ProductPendingCount: productPendingCount,
                VoucherApprovalCount: voucherApprovalCount,
                CategoryApprovalCount: categoryApprovalCount
            );

            ViewBag.DashboardStats = stats;


            // ===============================
            // Year Filter
            // ===============================

            ViewBag.Years = Enumerable
                .Range(DateTime.Now.Year - 5, 6)
                .Reverse()
                .ToList();

            ViewBag.SelectedYear = year;


            return View();
        }
    }
}