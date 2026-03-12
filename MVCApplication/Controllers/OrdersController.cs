using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MVCApplication.Models;
using MVCApplication.Services;

namespace MVCApplication.Controllers
{
    [Route("customer/orders")]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            IOrderService service,
            IProductService productService,
            ILogger<OrdersController> logger)
        {
            _orderService = service;
            _productService = productService;
            _logger = logger;
        }

        // GET /customer/orders
        [HttpGet("")]
        public async Task<IActionResult> Index(string? search)
        {
            var customerId = User.FindFirst("accountID")?.Value;
            if (string.IsNullOrWhiteSpace(customerId))
                return BadRequest("CustomerId required");

            var orders = await _orderService.GetOrdersByCustomerAsync(customerId);
            return View(orders);
        }

        // GET /customer/orders/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                var order = await _orderService.GetOrderDetailAsync(id);
                if (order == null)
                {
                    _logger.LogWarning("Order not found: {OrderId}", id);
                    return NotFound();
                }

                if (order.OrderItems != null)
                {
                    foreach (var item in order.OrderItems)
                    {
                        if (!string.IsNullOrWhiteSpace(item.ProductID))
                        {
                            try
                            {
                                var product = await _productService.GetProductDetailAsync(item.ProductID);
                                if (product != null)
                                {
                                    item.ProductName = product.ProductName;
                                    item.ProductImage = product.MainImage;
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Product API error while enriching order {OrderId}, product {ProductId}", id, item.ProductID);
                            }
                        }
                    }
                }

                return PartialView("_OrderDetailPartial", order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Order detail error for order {OrderId}", id);
#if DEBUG
                // In development return the exception message to help debugging (remove in production)
                return StatusCode(500, $"Failed to load order details: {ex.Message}");
#else
                return StatusCode(500, "Failed to load order details");
#endif
            }
        }

        // GET /customer/orders/search
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            string? orderId,
            DateTime? from,
            DateTime? to,
            string? paymentStatus,
            string? tabStatus)
        {
            var customerId = User.FindFirst("accountID")?.Value;
            if (string.IsNullOrWhiteSpace(customerId))
                return BadRequest("CustomerId required");

            var orders = await _orderService.SearchOrdersAsync(
                customerId,
                orderId,
                from,
                to,
                paymentStatus,
                tabStatus);

            return View("Index", orders);
        }

        // POST /customer/orders/{id}/cancel
        [HttpPost("{id}/cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(string id)
        {
            await _orderService.CancelOrderAsync(id, "Cancelled by customer");
            return RedirectToAction(nameof(Index));
        }
    }
}
