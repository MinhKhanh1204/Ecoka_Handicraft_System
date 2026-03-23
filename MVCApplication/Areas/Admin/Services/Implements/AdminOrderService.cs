using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using MVCApplication.Areas.Admin.DTOs;
using MVCApplication.Models.DTOs;

namespace MVCApplication.Areas.Admin.Services.Implements
{
    public class AdminOrderService : IAdminOrderService
    {
        private readonly HttpClient _http;
        private readonly ILogger<AdminOrderService> _logger;

        public AdminOrderService(HttpClient http, ILogger<AdminOrderService> logger)
        {
            _http = http;
            _logger = logger;
        }

        private const string BasePath = "admin/orders";

        // ===============================
        // REVENUE
        // ===============================
        public async Task<IEnumerable<RevenueByMonthDto>> GetRevenueByYearAsync(int year)
        {
            var resp = await _http.GetAsync($"{BasePath}/revenue?year={year}");
            resp.EnsureSuccessStatusCode();

            return await resp.Content.ReadFromJsonAsync<IEnumerable<RevenueByMonthDto>>()
                   ?? Enumerable.Empty<RevenueByMonthDto>();
        }

        // ===============================
        // GET ALL ORDERS FOR STAFF
        // ===============================
        public async Task<IEnumerable<Order>> GetAllOrdersForStaffAsync()
        {
            var resp = await _http.GetAsync(BasePath);
            resp.EnsureSuccessStatusCode();

            var orders = await resp.Content.ReadFromJsonAsync<IEnumerable<Order>>();
            return orders?
                .OrderByDescending(x => x.OrderDate)
                .ToList()
                ?? Enumerable.Empty<Order>();
        }

        // ===============================
        // SEARCH
        // ===============================
        public async Task<IEnumerable<Order>> SearchOrdersForStaffAsync(
            string? orderId,
            string? customerId,
            DateTime? from,
            DateTime? to,
            string? shippingStatus,
            string? paymentStatus)
        {
            orderId = Normalize(orderId);
            customerId = Normalize(customerId);
            shippingStatus = Normalize(shippingStatus);
            paymentStatus = Normalize(paymentStatus);

            if (from.HasValue && to.HasValue && from.Value.Date > to.Value.Date)
            {
                _logger.LogWarning("Invalid date range in SearchOrdersForStaffAsync: from={From}, to={To}", from, to);
                return Enumerable.Empty<Order>();
            }

            var allOrders = await GetAllOrdersForStaffAsync();
            var query = allOrders.AsQueryable();

            if (!string.IsNullOrWhiteSpace(orderId))
            {
                query = query.Where(x =>
                    !string.IsNullOrWhiteSpace(x.OrderID) &&
                    x.OrderID.Contains(orderId, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(customerId))
            {
                query = query.Where(x =>
                    !string.IsNullOrWhiteSpace(x.CustomerID) &&
                    x.CustomerID.Contains(customerId, StringComparison.OrdinalIgnoreCase));
            }

            if (from.HasValue)
            {
                query = query.Where(x =>
                    x.OrderDate.HasValue &&
                    x.OrderDate.Value.Date >= from.Value.Date);
            }

            if (to.HasValue)
            {
                query = query.Where(x =>
                    x.OrderDate.HasValue &&
                    x.OrderDate.Value.Date <= to.Value.Date);
            }

            if (!string.IsNullOrWhiteSpace(shippingStatus))
            {
                query = query.Where(x =>
                    string.Equals((x.ShippingStatus ?? "").Trim(), shippingStatus, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(paymentStatus))
            {
                query = query.Where(x =>
                    string.Equals((x.PaymentStatus ?? "").Trim(), paymentStatus, StringComparison.OrdinalIgnoreCase));
            }

            return query
                .OrderByDescending(x => x.OrderDate)
                .ThenByDescending(x => x.OrderID)
                .ToList();
        }

        // ===============================
        // UPDATE SHIPPING STATUS
        // ===============================
        public async Task<bool> UpdateOrderStatusAsync(string orderId, string newStatus, string staffId)
        {
            if (string.IsNullOrWhiteSpace(orderId) || string.IsNullOrWhiteSpace(newStatus))
                return false;

            var payload = new
            {
                NewStatus = newStatus.Trim(),
                StaffId = string.IsNullOrWhiteSpace(staffId) ? "ADMIN" : staffId.Trim()
            };

            var resp = await _http.PutAsJsonAsync(
                $"{BasePath}/{Uri.EscapeDataString(orderId.Trim())}/status",
                payload
            );

            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "UpdateOrderStatusAsync failed. OrderId={OrderId}, NewStatus={NewStatus}, StatusCode={StatusCode}",
                    orderId, newStatus, resp.StatusCode);
            }

            return resp.IsSuccessStatusCode;
        }

        // ===============================
        // UPDATE PAYMENT STATUS
        // ===============================
        public async Task<bool> UpdatePaymentStatusAsync(string orderId, string newPaymentStatus, string staffId)
        {
            if (string.IsNullOrWhiteSpace(orderId) || string.IsNullOrWhiteSpace(newPaymentStatus))
                return false;

            var payload = new
            {
                NewPaymentStatus = newPaymentStatus.Trim(),
                StaffId = string.IsNullOrWhiteSpace(staffId) ? "ADMIN" : staffId.Trim()
            };

            var resp = await _http.PutAsJsonAsync(
                $"{BasePath}/{Uri.EscapeDataString(orderId.Trim())}/payment-status",
                payload
            );

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "UpdatePaymentStatusAsync failed. OrderId={OrderId}, NewPaymentStatus={NewPaymentStatus}, StatusCode={StatusCode}, Body={Body}",
                    orderId, newPaymentStatus, resp.StatusCode, body);
            }

            return resp.IsSuccessStatusCode;
        }

        // ===============================
        // STAFF ORDER DETAIL
        // ===============================
        public async Task<Order?> GetOrderDetailForStaffAsync(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                return null;

            var resp = await _http.GetAsync($"{BasePath}/{Uri.EscapeDataString(orderId.Trim())}");

            if (resp.StatusCode == HttpStatusCode.NotFound)
                return null;

            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<Order?>();
        }

        // ===============================
        // GENERAL GET BY ID
        // ===============================
        public async Task<Order?> GetByIdAsync(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                return null;

            var resp = await _http.GetAsync($"{BasePath}/{Uri.EscapeDataString(orderId.Trim())}");

            if (resp.StatusCode == HttpStatusCode.NotFound)
                return null;

            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<Order?>();
        }

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        public async Task<bool> UpdatePaymentStatusAsync(
     string orderId,
     string paymentMethod,
     string newPaymentStatus,
     string status,
     string note,
     string staffId)
        {
            if (string.IsNullOrWhiteSpace(orderId) ||
                string.IsNullOrWhiteSpace(paymentMethod) ||
                string.IsNullOrWhiteSpace(newPaymentStatus) ||
                string.IsNullOrWhiteSpace(status))
            {
                return false;
            }

            var payload = new
            {
                paymentMethod = paymentMethod.Trim(),
                newPaymentStatus = newPaymentStatus.Trim(),
                status = status.Trim(),
                note = string.IsNullOrWhiteSpace(note) ? "Refund after order cancellation." : note.Trim(),
                staffId = string.IsNullOrWhiteSpace(staffId) ? "STF001" : staffId.Trim()
            };

            var resp = await _http.PutAsJsonAsync(
                $"{BasePath}/{Uri.EscapeDataString(orderId.Trim())}/payment-status",
                payload
            );

            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "UpdatePaymentStatusAsync failed. OrderId={OrderId}, StatusCode={StatusCode}",
                    orderId, resp.StatusCode);
            }

            return resp.IsSuccessStatusCode;
        }
    }
}