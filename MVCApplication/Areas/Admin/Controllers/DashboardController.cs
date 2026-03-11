using Microsoft.AspNetCore.Mvc;
using MVCApplication.Areas.Admin.Services;

namespace MVCApplication.Areas.Admin.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IAdminOrderService _adminOrderService;

        public DashboardController(IAdminOrderService adminOrderService)
        {
            _adminOrderService = adminOrderService;
        }

        [Area("Admin")]
        public async Task<IActionResult> Index(int? revenueYear)
        {
            var year = revenueYear ?? DateTime.Now.Year;

            var revenueData = await _adminOrderService.GetRevenueByYearAsync(year);

            ViewBag.RevenueData = revenueData;
            ViewBag.SelectedRevenueYear = year;

            return View();
        }
    }
}
