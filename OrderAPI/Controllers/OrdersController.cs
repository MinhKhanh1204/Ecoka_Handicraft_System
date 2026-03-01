using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderAPI.DTOs;
using OrderAPI.Services;

namespace OrderAPI.Controllers
{
    [Route("api/customer/orders/")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // ================= GET ALL =================
        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            // Lấy từ Claim (khuyên dùng)
            var customerId = "CUS001";

            // Hoặc từ Session
            // var customerId = HttpContext.Session.GetString("CustomerId");

            if (string.IsNullOrWhiteSpace(customerId))
                return Unauthorized();

            var orders = await _orderService.GetOrdersByCustomerAsync(customerId);

            return Ok(orders);
        }

        // ================= SEARCH =================
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string? orderId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string? paymentStatus,
            [FromQuery] string? tabStatus)
        {
            var customerId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(customerId))
                return Unauthorized();

            var orders = await _orderService.SearchOrdersAsync(
                customerId,
                orderId,
                from,
                to,
                paymentStatus,
                tabStatus);

            return Ok(orders);
        }

        // ================= DETAIL =================
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetDetail(string orderId)
        {
            var order = await _orderService.GetOrderDetailAsync(orderId);
            if (order == null) return NotFound();

            return Ok(order);
        }

        // ================= CREATE =================
        [HttpPost]
        public async Task<IActionResult> Create(OrderCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _orderService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetDetail),
                new { orderId = created.OrderID },
                created);
        }

        // ================= CANCEL =================
        [HttpPut("{orderId}/cancel")]
        public async Task<IActionResult> Cancel(string orderId, [FromBody] string reason)
        {
            var result = await _orderService.CancelOrderAsync(orderId, reason);
            if (!result) return BadRequest("Cannot cancel this order");

            return NoContent();
        }

        // ================= CHECK PURCHASE =================
        [HttpGet("has-purchased")]
        public async Task<IActionResult> HasPurchased(
            [FromQuery] string productId)
        {
            var customerId = "CUS001"; // sửa lại
            if (string.IsNullOrEmpty(customerId))
                return Unauthorized();

            var result = await _orderService
                .HasCustomerPurchasedProductAsync(customerId, productId);

            return Ok(result);
        }
    }
}
