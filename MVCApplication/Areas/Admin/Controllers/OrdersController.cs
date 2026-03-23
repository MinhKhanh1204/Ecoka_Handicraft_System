using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MVCApplication.Areas.Admin.Services;
using MVCApplication.Models.DTOs;

namespace MVCApplication.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee")]
    public class OrdersController : Controller
    {
        private readonly IAdminOrderService _service;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IAdminOrderService service, ILogger<OrdersController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private static List<string> GetAllowedNextStatuses(string? currentStatus, string? paymentStatus)
        {
            var shipping = string.IsNullOrWhiteSpace(currentStatus) ? "Pending" : currentStatus.Trim();
            var payment = string.IsNullOrWhiteSpace(paymentStatus) ? "Pending" : paymentStatus.Trim();

            var isPaid = string.Equals(payment, "Paid", StringComparison.OrdinalIgnoreCase);

            if (isPaid)
            {
                return shipping switch
                {
                    "Pending" => new List<string> { "Shipping", "Cancelled" },
                    "Approved" => new List<string> { "Shipping", "Cancelled" },
                    "Shipping" => new List<string> { "Delivered", "Cancelled" },
                    _ => new List<string>()
                };
            }

            return shipping switch
            {
                "Pending" => new List<string> { "Approved", "Cancelled" },
                "Approved" => new List<string> { "Shipping", "Cancelled" },
                "Shipping" => new List<string> { "Delivered", "Cancelled" },
                _ => new List<string>()
            };
        }

        private static object BuildPagedResult(
            IEnumerable<Order> orders,
            int page,
            int pageSize,
            string? orderId,
            string? customerId,
            DateTime? from,
            DateTime? to,
            string? shippingStatus,
            string? paymentStatus)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            if (pageSize > 100) pageSize = 10;

            var totalItems = orders.Count();
            var totalPages = totalItems == 0 ? 1 : (int)Math.Ceiling((double)totalItems / pageSize);

            if (page > totalPages) page = totalPages;

            var pagedOrders = orders
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o =>
                {
                    var isFinal =
                        string.Equals(o.ShippingStatus, "Delivered", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(o.ShippingStatus, "Cancelled", StringComparison.OrdinalIgnoreCase);

                    return new
                    {
                        o.OrderID,
                        o.CustomerID,
                        o.OrderDate,
                        o.TotalAmount,
                        o.PaymentStatus,
                        o.ShippingStatus,
                        CanUpdate = !isFinal,
                        NextStatuses = GetAllowedNextStatuses(o.ShippingStatus, o.PaymentStatus)
                    };
                })
                .ToList();

            return new
            {
                items = pagedOrders,
                pagination = new
                {
                    currentPage = page,
                    pageSize,
                    totalItems,
                    totalPages
                },
                filters = new
                {
                    orderId,
                    customerId,
                    from = from?.ToString("yyyy-MM-dd"),
                    to = to?.ToString("yyyy-MM-dd"),
                    shippingStatus,
                    paymentStatus
                }
            };
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? orderId,
            string? customerId,
            DateTime? from,
            DateTime? to,
            string? shippingStatus,
            string? paymentStatus,
            int page = 1,
            int pageSize = 10)
        {
            ViewBag.OrderId = orderId;
            ViewBag.CustomerId = customerId;
            ViewBag.From = from?.ToString("yyyy-MM-dd");
            ViewBag.To = to?.ToString("yyyy-MM-dd");
            ViewBag.ShippingStatus = shippingStatus;
            ViewBag.PaymentStatus = paymentStatus;

            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            if (pageSize > 100) pageSize = 10;

            IEnumerable<Order> orders = Enumerable.Empty<Order>();

            try
            {
                if (from.HasValue && to.HasValue && from.Value.Date > to.Value.Date)
                {
                    TempData["Error"] = "Ngày bắt đầu không được lớn hơn ngày kết thúc.";
                    ViewBag.CurrentPage = 1;
                    ViewBag.PageSize = pageSize;
                    ViewBag.TotalItems = 0;
                    ViewBag.TotalPages = 1;
                    return View(Enumerable.Empty<Order>());
                }

                var hasFilter =
                    !string.IsNullOrWhiteSpace(orderId) ||
                    !string.IsNullOrWhiteSpace(customerId) ||
                    from != null ||
                    to != null ||
                    !string.IsNullOrWhiteSpace(shippingStatus) ||
                    !string.IsNullOrWhiteSpace(paymentStatus);

                orders = hasFilter
                    ? await _service.SearchOrdersForStaffAsync(orderId, customerId, from, to, shippingStatus, paymentStatus)
                        ?? Enumerable.Empty<Order>()
                    : await _service.GetAllOrdersForStaffAsync()
                        ?? Enumerable.Empty<Order>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading orders from API");
                TempData["Error"] = "Không tải được danh sách đơn hàng.";
                orders = Enumerable.Empty<Order>();
            }

            var totalItems = orders.Count();
            var totalPages = totalItems == 0 ? 1 : (int)Math.Ceiling((double)totalItems / pageSize);
            if (page > totalPages) page = totalPages;

            var pagedOrders = orders
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = totalPages;

            return View(pagedOrders);
        }

        [HttpGet]
        public async Task<IActionResult> SearchAjax(
            string? orderId,
            string? customerId,
            DateTime? from,
            DateTime? to,
            string? shippingStatus,
            string? paymentStatus,
            int page = 1,
            int pageSize = 10)
        {
            try
            {
                if (page <= 0) page = 1;
                if (pageSize <= 0) pageSize = 10;
                if (pageSize > 100) pageSize = 10;

                if (from.HasValue && to.HasValue && from.Value.Date > to.Value.Date)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Ngày bắt đầu không được lớn hơn ngày kết thúc."
                    });
                }

                var hasFilter =
                    !string.IsNullOrWhiteSpace(orderId) ||
                    !string.IsNullOrWhiteSpace(customerId) ||
                    from != null ||
                    to != null ||
                    !string.IsNullOrWhiteSpace(shippingStatus) ||
                    !string.IsNullOrWhiteSpace(paymentStatus);

                var orders = hasFilter
                    ? await _service.SearchOrdersForStaffAsync(orderId, customerId, from, to, shippingStatus, paymentStatus)
                        ?? Enumerable.Empty<Order>()
                    : await _service.GetAllOrdersForStaffAsync()
                        ?? Enumerable.Empty<Order>();

                var result = BuildPagedResult(
                    orders,
                    page,
                    pageSize,
                    orderId,
                    customerId,
                    from,
                    to,
                    shippingStatus,
                    paymentStatus);

                return Json(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching orders");
                return Json(new
                {
                    success = false,
                    message = "Không tải được danh sách đơn hàng."
                });
            }
        }

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

                return PartialView("_OrderDetails", dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get order detail {OrderId}", orderId);
                return Content("<div class='alert alert-danger'>Không tải được chi tiết đơn hàng.</div>", "text/html");
            }
        }

        [HttpGet]
        public IActionResult GetNextStatuses(string currentStatus, string payment)
        {
            return Json(new
            {
                success = true,
                statuses = GetAllowedNextStatuses(currentStatus, payment)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(string orderId, string newStatus)
        {
            if (string.IsNullOrWhiteSpace(orderId) || string.IsNullOrWhiteSpace(newStatus))
            {
                return Json(new
                {
                    success = false,
                    message = "Thiếu dữ liệu cập nhật."
                });
            }

            try
            {
                newStatus = newStatus.Trim();

                var staffId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "STF001";

                var order = await _service.GetOrderDetailForStaffAsync(orderId);
                if (order == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không tìm thấy đơn hàng."
                    });
                }

                var currentStatus = string.IsNullOrWhiteSpace(order.ShippingStatus)
                    ? "Pending"
                    : order.ShippingStatus.Trim();

                var allowedNextStatuses = GetAllowedNextStatuses(currentStatus, order.PaymentStatus);

                if (!allowedNextStatuses.Contains(newStatus, StringComparer.OrdinalIgnoreCase))
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Không thể chuyển từ {currentStatus} sang {newStatus}."
                    });
                }

                var updatedShipping = await _service.UpdateOrderStatusAsync(orderId, newStatus, staffId);
                if (!updatedShipping)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Cập nhật trạng thái giao hàng thất bại."
                    });
                }

                if (string.Equals(order.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(newStatus, "Cancelled", StringComparison.OrdinalIgnoreCase))
                {
                    var paymentMethod = string.IsNullOrWhiteSpace(order.PaymentMethod)
                        ? "Online"
                        : order.PaymentMethod.Trim();

                    var updatedPayment = await _service.UpdatePaymentStatusAsync(
                        orderId: orderId,
                        paymentMethod: paymentMethod,
                        newPaymentStatus: "Refunded",
                        status: "Canceled",
                        note: "Refund after order cancellation.",
                        staffId: staffId
                    );

                    if (!updatedPayment)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Đã hủy đơn nhưng cập nhật hoàn tiền thất bại."
                        });
                    }
                }

                // Đọc lại order mới nhất từ backend
                var refreshedOrder = await _service.GetOrderDetailForStaffAsync(orderId);
                var resultingShippingStatus = refreshedOrder?.ShippingStatus ?? newStatus;
                var resultingPaymentStatus = refreshedOrder?.PaymentStatus ?? order.PaymentStatus;

                var canUpdateAfter =
                    !string.Equals(resultingShippingStatus, "Delivered", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(resultingShippingStatus, "Cancelled", StringComparison.OrdinalIgnoreCase);

                return Json(new
                {
                    success = true,
                    message =
                        string.Equals(order.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(newStatus, "Cancelled", StringComparison.OrdinalIgnoreCase)
                            ? "Đơn đã được hủy và chuyển sang hoàn tiền."
                            : "Cập nhật trạng thái thành công.",
                    orderId,
                    newStatus = resultingShippingStatus,
                    newPaymentStatus = resultingPaymentStatus,
                    canUpdate = canUpdateAfter,
                    nextStatuses = GetAllowedNextStatuses(resultingShippingStatus, resultingPaymentStatus)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderId}", orderId);
                return Json(new
                {
                    success = false,
                    message = "Lỗi hệ thống."
                });
            }
        }

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