using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderAPI.DTOs;
using OrderAPI.Services;
using Microsoft.Extensions.Logging;

namespace OrderAPI.Controllers
{
    [Route("api/customer/orders/")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        // ================= GET ALL =================
        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            // Lấy từ Claim (khuyên dùng)
            var customerId = User.FindFirst("accountID")?.Value;

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
            var customerId = User.FindFirst("accountID")?.Value;
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
            var order = await _orderService.GetByIdAsync(orderId);
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

        // ================= UPDATE PAYMENT =================
        [HttpPut("{orderId}/payment-status")]
        public async Task<IActionResult> UpdatePaymentStatus(string orderId, [FromBody] PaymentStatusUpdateDto dto)
        {
            await _orderService.UpdatePaymentStatusAsync(orderId, dto.PaymentMethod, dto.Status, dto.Note);
            return NoContent();
        }

        // ================= CHECK PURCHASE =================
        [HttpGet("has-purchased")]
        public async Task<IActionResult> HasPurchased(
            [FromQuery] string productId,
            [FromQuery] string? customerId)
        {
            var claimId = User.FindFirst("accountID")?.Value;
            _logger.LogDebug("HasPurchased called. ClaimId='{ClaimId}', QueryCustomerId='{QueryCustomerId}', ProductId='{ProductId}'", claimId, customerId, productId);

            if (string.IsNullOrEmpty(claimId) && string.IsNullOrEmpty(customerId))
            {
                _logger.LogWarning("HasPurchased unauthorized: missing accountID claim and customerId query");
                return Unauthorized();
            }

            if (!string.IsNullOrEmpty(claimId) && !string.IsNullOrEmpty(customerId) && !string.Equals(claimId, customerId, StringComparison.Ordinal))
            {
                _logger.LogWarning("HasPurchased forbidden: claimId and query customerId mismatch (Claim={Claim}, Query={Query})", claimId, customerId);
                return Forbid();
            }

            var effectiveCustomerId = !string.IsNullOrEmpty(claimId) ? claimId : customerId!;

            var result = await _orderService
                .HasCustomerPurchasedProductAsync(effectiveCustomerId, productId);

            _logger.LogInformation("HasPurchased result for CustomerId='{CustomerId}', ProductId='{ProductId}' => {Result}", effectiveCustomerId, productId, result);

            return Ok(result);
        }
    }
}
