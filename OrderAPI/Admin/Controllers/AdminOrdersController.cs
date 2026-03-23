using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrderAPI.Admin.Services;
using OrderAPI.Admin.DTOs;
using OrderAPI.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OrderAPI.Admin.Controllers
{
    [ApiController]
    [Route("api/admin/orders")]
    public class AdminOrdersController : ControllerBase
    {
        private readonly IAdminOrderService _service;
        private readonly ILogger<AdminOrdersController> _logger;

        public AdminOrdersController(IAdminOrderService service, ILogger<AdminOrdersController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // GET api/admin/orders/revenue?year=2026
        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenueByYear([FromQuery] int? year, CancellationToken ct = default)
        {
            if (year.HasValue && year <= 0)
                return BadRequest(new { Error = "Year must be a positive integer." });

            var queryYear = year ?? DateTime.UtcNow.Year;

            try
            {
                var data = await _service.GetRevenueByYearAsync(queryYear);

                if (data == null || !data.Any())
                {
                    _logger.LogInformation("No revenue data found for year {Year}.", queryYear);
                    return NotFound(new { Message = "No revenue data found for the specified year." });
                }

                var normalized = Enumerable.Range(1, 12)
                    .Select(m => data.FirstOrDefault(d => d.Month == m) ?? new RevenueByMonthDto { Month = m, Revenue = 0m })
                    .OrderBy(d => d.Month);

                return Ok(normalized);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Request cancelled while getting revenue for year {Year}.", queryYear);
                return StatusCode(499);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving revenue for year {Year}.", queryYear);
                return StatusCode(500, new { Error = "An internal error occurred while retrieving revenue data." });
            }
        }

        // STAFF: list all orders (for staff UI)
        [HttpGet("")]
        public async Task<IActionResult> GetAllOrdersForStaff(CancellationToken ct = default)
        {
            try
            {
                var orders = await _service.GetAllOrdersForStaffAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all orders for staff.");
                return StatusCode(500, new { Error = "Failed to fetch orders." });
            }
        }

        // STAFF: search orders
        [HttpGet("search")]
        public async Task<IActionResult> SearchOrdersForStaff(
            [FromQuery] string? orderId,
            [FromQuery] string? customerId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string? shippingStatus,
            [FromQuery] string? paymentStatus,
            CancellationToken ct = default)
        {
            try
            {
                var result = await _service.SearchOrdersForStaffAsync(orderId, customerId, from, to, shippingStatus, paymentStatus);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching orders for staff.");
                return StatusCode(500, new { Error = "Failed to search orders." });
            }
        }

        // STAFF: get order details for staff
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderDetailForStaff([FromRoute] string orderId, CancellationToken ct = default)
        {
            try
            {
                var dto = await _service.GetOrderDetailForStaffAsync(orderId);
                if (dto == null) return NotFound(new { Message = "Order not found." });
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order detail for staff. OrderId: {OrderId}", orderId);
                return StatusCode(500, new { Error = "Failed to retrieve order detail." });
            }
        }

        // ================= UPDATE PAYMENT =================
        [HttpPut("{orderId}/payment-status")]
        public async Task<IActionResult> UpdatePaymentStatus(string orderId, [FromBody] PaymentStatusUpdateDto dto)
        {
            if (dto == null)
                return BadRequest(new { success = false, message = "Request body is required." });

            // Log incoming payload to help diagnose why MVC call returns non-success.
            _logger.LogDebug("Admin UpdatePaymentStatus called. OrderId={OrderId}, Payload={Payload}",
                orderId, System.Text.Json.JsonSerializer.Serialize(dto));

            try
            {
                var paymentStatus = dto.NewPaymentStatus?.Trim() ?? dto.Status?.Trim() ?? string.Empty;
                var note = dto.Note?.Trim();
                var staffId = dto.StaffId?.Trim();

                var success = await _service.UpdatePaymentStatusAsync(orderId, paymentStatus, note, staffId);

                if (!success)
                {
                    _logger.LogWarning("UpdatePaymentStatus failed for OrderId={OrderId}. Payload={Payload}", orderId,
                        System.Text.Json.JsonSerializer.Serialize(dto));

                    return NotFound(new { success = false, message = "Order not found or update not applied." });
                }

                return Ok(new { success = true, message = "Payment status updated." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while updating payment status for OrderId={OrderId}. Payload={Payload}", orderId,
                    System.Text.Json.JsonSerializer.Serialize(dto));
                return StatusCode(500, new { success = false, message = "Internal error while updating payment status." });
            }
        }

        // Update order shipping/status by staff
        public record UpdateStatusRequest(string NewStatus, string StaffId);

        [HttpPut("{orderId}/status")]
        public async Task<IActionResult> UpdateOrderStatus([FromRoute] string orderId, [FromBody] UpdateStatusRequest req, CancellationToken ct = default)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.NewStatus))
                return BadRequest(new { Error = "NewStatus is required." });

            try
            {
                var success = await _service.UpdateOrderStatusAsync(orderId, req.NewStatus, req.StaffId);
                if (!success) return NotFound(new { Message = "Order not found." });
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status. OrderId: {OrderId}", orderId);
                return StatusCode(500, new { Error = "Failed to update order status." });
            }
        }
    }
}
