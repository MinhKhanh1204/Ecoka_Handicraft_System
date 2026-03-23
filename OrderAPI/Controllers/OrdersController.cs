using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderAPI.DTOs;
using OrderAPI.Services;
using Microsoft.Extensions.Logging;
using OrderAPI.Models;

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
            if (dto == null)
                return BadRequest("Request body is required.");

            // UI posts { PaymentMethod, Status, Note } — prefer PaymentMethod, fall back to NewPaymentStatus
            var paymentMethod = !string.IsNullOrWhiteSpace(dto.PaymentMethod)
                ? dto.PaymentMethod.Trim()
                : dto.NewPaymentStatus?.Trim() ?? string.Empty;

            await _orderService.UpdatePaymentStatusAsync(orderId, paymentMethod, dto.Status, dto.Note);
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

        // ================= VOUCHER USAGE =================
        [HttpGet("voucher-usage/{voucherId}")]
        public async Task<IActionResult> GetVoucherUsage(int voucherId)
        {
            var customerId = User.FindFirst("accountID")?.Value;
            if (string.IsNullOrEmpty(customerId))
                return Unauthorized();

            var count = await _orderService.GetVoucherUsageCountAsync(customerId, voucherId);
            return Ok(count);
        }

        // ================= CONFIRM RECEIPT (CUSTOMER) =================
        [HttpPost("{orderId}/confirm-receipt")]
        public async Task<IActionResult> ConfirmReceipt(string orderId)
        {
            var customerId = User.FindFirst("accountID")?.Value;
            if (string.IsNullOrWhiteSpace(customerId))
                return Unauthorized();

            try
            {
                var success = await _orderService.ConfirmReceiptAsync(orderId, customerId);
                if (!success)
                    return BadRequest(new { success = false, message = "Cannot confirm receipt (order not found, not owner, or invalid state)." });

                return Ok(new { success = true, message = "Order marked as paid and delivered." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming receipt for OrderId: {OrderId}", orderId);
                return StatusCode(500, new { success = false, message = "Internal error." });
            }
        }
    }
}
