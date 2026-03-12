using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MVCApplication.Areas.Admin.Services;
using MVCApplication.Models.DTOs;
using System.Security.Claims;

namespace MVCApplication.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrdersController : Controller
    {
        private readonly IAdminOrderService _service;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IAdminOrderService service, ILogger<OrdersController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // GET: /Admin/Orders
        [HttpGet]
        public async Task<IActionResult> Index(
            string? orderId,
            string? customerName,
            DateTime? from,
            DateTime? to,
            string? shippingStatus,
            string? paymentStatus,
            int page = 1,
            int pageSize = 10)
        {
            IEnumerable<Order> orders = new List<Order>();

            try
            {
                bool hasFilter =
                    !string.IsNullOrWhiteSpace(orderId) ||
                    !string.IsNullOrWhiteSpace(customerName) ||
                    from.HasValue ||
                    to.HasValue ||
                    !string.IsNullOrWhiteSpace(shippingStatus) ||
                    !string.IsNullOrWhiteSpace(paymentStatus);

                if (hasFilter)
                {
                    orders = await _service.SearchOrdersForStaffAsync(
                        orderId,
                        customerName,
                        from,
                        to,
                        shippingStatus,
                        paymentStatus
                    ) ?? new List<Order>();
                }
                else
                {
                    orders = await _service.GetAllOrdersForStaffAsync()
                             ?? new List<Order>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading orders from API");
                TempData["Error"] = "Cannot load orders from API.";
            }

            // pagination
            var totalItems = orders.Count();

            var pagedOrders = orders
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // giữ lại filter
            ViewBag.OrderId = orderId;
            ViewBag.CustomerName = customerName;
            ViewBag.From = from?.ToString("yyyy-MM-dd");
            ViewBag.To = to?.ToString("yyyy-MM-dd");
            ViewBag.ShippingStatus = shippingStatus;
            ViewBag.PaymentStatus = paymentStatus;

            return View(pagedOrders);
        }

        // GET: /Admin/Orders/Details/{orderId}
        [HttpGet]
        public async Task<IActionResult> Details(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                return BadRequest();

            try
            {
                var dto = await _service.GetOrderDetailForStaffAsync(orderId);

                if (dto == null)
                    return NotFound();

                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get order detail {OrderId}", orderId);
                TempData["Error"] = "Cannot load order detail";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Admin/Orders/UpdateStatus
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(string orderId, string newStatus)
        {
            if (string.IsNullOrWhiteSpace(orderId) ||
                string.IsNullOrWhiteSpace(newStatus))
            {
                return Json(new { success = false, message = "Invalid request" });
            }

            try
            {
                var staffId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "ADMIN";

                var result = await _service.UpdateOrderStatusAsync(
                    orderId,
                    newStatus,
                    staffId
                );

                if (result)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Order status updated"
                    });
                }

                return Json(new
                {
                    success = false,
                    message = "Order not found"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderId}", orderId);

                return Json(new
                {
                    success = false,
                    message = "Update failed"
                });
            }
        }

        // GET: /Admin/Orders/Revenue?year=2026
        [HttpGet]
        public async Task<IActionResult> Revenue(int? year)
        {
            try
            {
                int y = year ?? DateTime.UtcNow.Year;

                var revenue = await _service.GetRevenueByYearAsync(y);

                return Json(revenue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get revenue data");
                return StatusCode(500);
            }
        }

        // GET: /Admin/Orders/Get/{orderId}
        [HttpGet("Get/{orderId}")]
        public async Task<IActionResult> Get(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                return BadRequest();

            try
            {
                var order = await _service.GetByIdAsync(orderId);

                if (order == null)
                    return NotFound();

                return Json(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching order {OrderId}", orderId);
                return StatusCode(500);
            }
        }
    }
}