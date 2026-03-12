using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using MVCApplication.Areas.Admin.DTOs;
using MVCApplication.Models.DTOs;

namespace MVCApplication.Areas.Admin.Services.Implements
{
    public class AdminOrderService : IAdminOrderService
    {
        private readonly HttpClient _http;

        public AdminOrderService(HttpClient http)
        {
            _http = http;
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
            string? customerName,
            DateTime? from,
            DateTime? to,
            string? shippingStatus,
            string? paymentStatus)
        {
            var query = new Dictionary<string, string?>()
            {
                ["orderId"] = orderId,
                ["customerName"] = customerName,
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

            var resp = await _http.GetAsync(uri);

            resp.EnsureSuccessStatusCode();

            return await resp.Content.ReadFromJsonAsync<IEnumerable<Order>>()
                   ?? Enumerable.Empty<Order>();
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