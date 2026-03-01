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
        private readonly IOrderService _service;

        public OrdersController(IOrderService service)
        {
            _service = service;
        }

        // Danh sách order của customer
        public async Task<IActionResult> Index(string customerId)
        {
            //if (string.IsNullOrWhiteSpace(customerId))
            //    return BadRequest("CustomerId required");

            var orders = await _service.GetOrdersByCustomerAsync(customerId);
            return View(orders);
        }

        // Chi tiết order
        public async Task<IActionResult> Details(string id)
        {
            var order = await _service.GetOrderDetailAsync(id);
            if (order == null) return NotFound();

            return PartialView("_OrderDetailPartial", order);
        }

        // Search
        public async Task<IActionResult> Search(
            string customerId,
            string? orderId,
            DateTime? from,
            DateTime? to,
            string? paymentStatus,
            string? tabStatus)
        {
            var orders = await _service.SearchOrdersAsync(
                customerId, orderId, from, to, paymentStatus, tabStatus);

            return View("Index", orders);
        }

        // Cancel
        [HttpPost]
        public async Task<IActionResult> Cancel(string id, string? returnCustomerId = null)
        {
            await _service.CancelOrderAsync(id, "Cancelled by customer");
            return RedirectToAction(nameof(Index), new { customerId = returnCustomerId });
        }
    }
}
