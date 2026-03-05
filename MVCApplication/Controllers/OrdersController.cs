using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MVCApplication.Models;
using MVCApplication.Services;

namespace MVCApplication.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;


        public OrdersController(IOrderService service, IProductService productService)
        {
            _orderService = service;
            _productService = productService;
        }

        // Danh sách order của customer
        public async Task<IActionResult> Index(string? search)
        {
            var customerId = User.FindFirst("accountID")?.Value;
            if (string.IsNullOrWhiteSpace(customerId))
                return BadRequest("CustomerId required");

            var orders = await _orderService
                .GetOrdersByCustomerAsync(customerId);

            return View(orders);
        }

        // Chi tiết order
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                var order = await _orderService.GetOrderDetailAsync(id);
                if (order == null)
                    return NotFound();

                if (order.OrderItems != null)
                {
                    foreach (var item in order.OrderItems)
                    {
                        if (!string.IsNullOrWhiteSpace(item.ProductID))
                        {
                            try
                            {
                                var product = await _productService
                                    .GetProductDetailAsync(item.ProductID);

                                if (product != null)
                                {
                                    item.ProductName = product.ProductName;
                                    item.ProductImage = product.MainImage;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Product API error: " + ex.Message);
                            }
                        }
                    }
                }

                return PartialView("_OrderDetailPartial", order);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Order detail error: " + ex.Message);
                return StatusCode(500, "Failed to load order details");
            }
        }

        // Search
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

        // Cancel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(string id)
        {
            await _orderService.CancelOrderAsync(id, "Cancelled by customer");

            return RedirectToAction(nameof(Index));
        }
    }
}
