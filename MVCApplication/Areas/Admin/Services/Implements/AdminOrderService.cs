using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
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

        // ?úng base API
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
            var resp = await _http.GetAsync($"{BasePath}");
            resp.EnsureSuccessStatusCode();

            return await resp.Content.ReadFromJsonAsync<IEnumerable<Order>>()
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
            var query = new Dictionary<string, string?>()
            {
                ["orderId"] = orderId,
                ["customerId"] = customerId,
                ["from"] = from?.ToString("yyyy-MM-dd"),
                ["to"] = to?.ToString("yyyy-MM-dd"),
                ["shippingStatus"] = shippingStatus,
                ["paymentStatus"] = paymentStatus
            };

            var filtered = query
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .ToDictionary(x => x.Key, x => x.Value);

            var uri = QueryHelpers.AddQueryString(
                $"{BasePath}/search",
                filtered
            );

            _logger.LogInformation("AdminOrderService.SearchOrdersForStaffAsync -> GET {Uri}", uri);

            try
            {
                var resp = await _http.GetAsync(uri);

                // if backend returns 204 NoContent -> treat as empty result
                if (resp.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    _logger.LogDebug("Search returned 204 NoContent for {Uri}", uri);
                    return Enumerable.Empty<Order>();
                }

                if (!resp.IsSuccessStatusCode)
                {
                    var body = await resp.Content.ReadAsStringAsync();
                    _logger.LogWarning("SearchOrdersForStaffAsync returned {Status} {Reason} for {Uri}. Body: {Body}",
                        resp.StatusCode, resp.ReasonPhrase, uri, body);
                    return Enumerable.Empty<Order>();
                }

                // Safe read with try/catch to avoid thrown when content is empty/invalid JSON
                try
                {
                    var result = await resp.Content.ReadFromJsonAsync<IEnumerable<Order>>();
                    return result ?? Enumerable.Empty<Order>();
                }
                catch (Exception ex)
                {
                    var body = await resp.Content.ReadAsStringAsync();
                    _logger.LogError(ex, "Failed to deserialize search response for {Uri}. Response body: {Body}", uri, body);
                    return Enumerable.Empty<Order>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HTTP request failed for search {Uri}", uri);
                return Enumerable.Empty<Order>();
            }
        }

        // ===============================
        // UPDATE STATUS
        // ===============================
        public async Task<bool> UpdateOrderStatusAsync(string orderId, string newStatus, string staffId)
        {
            var payload = new
            {
                NewStatus = newStatus,
                StaffId = staffId
            };

            var resp = await _http.PutAsJsonAsync(
                $"{BasePath}/{Uri.EscapeDataString(orderId)}/status",
                payload
            );

            return resp.IsSuccessStatusCode;
        }

        // ===============================
        // STAFF ORDER DETAIL
        // ===============================
        public async Task<Order?> GetOrderDetailForStaffAsync(string orderId)
        {
            var resp = await _http.GetAsync(
                $"{BasePath}/{Uri.EscapeDataString(orderId)}"
            );

            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            resp.EnsureSuccessStatusCode();

            return await resp.Content.ReadFromJsonAsync<Order?>();
        }

        // ===============================
        // GENERAL GET BY ID
        // ===============================
        public async Task<Order?> GetByIdAsync(string orderId)
        {
            var resp = await _http.GetAsync(
                $"{BasePath}/{Uri.EscapeDataString(orderId)}"
            );

            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            resp.EnsureSuccessStatusCode();

            return await resp.Content.ReadFromJsonAsync<Order?>();
        }
    }
}