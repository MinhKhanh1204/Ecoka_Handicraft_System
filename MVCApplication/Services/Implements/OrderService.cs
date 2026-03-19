using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using MVCApplication.Models.DTOs;

namespace MVCApplication.Services.Implements
{
    public class OrderService : IOrderService
    {
        private readonly HttpClient _http;

        public OrderService(HttpClient http)
        {
            _http = http;
        }

        private async Task<T?> GetOrNullAsync<T>(string uri) where T : class
        {
            var resp = await _http.GetAsync(uri);
            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<T?>();
        }

        public async Task<IEnumerable<Order>> GetOrdersByCustomerAsync(string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                return Enumerable.Empty<Order>();

            // call API route (API controllers are under "api/customer/orders")
            var response = await _http.GetAsync("customer/orders");

            if (!response.IsSuccessStatusCode)
                return Enumerable.Empty<Order>();

            return await response.Content.ReadFromJsonAsync<IEnumerable<Order>>()
                   ?? Enumerable.Empty<Order>();
        }

        public async Task<IEnumerable<Order>> SearchOrdersAsync(string customerId, string? orderId, DateTime? from, DateTime? to, string? paymentStatus, string? tabStatus)
        {
            var q = new Dictionary<string, string?>
            {
                // API reads customerId from JWT/claims; including it in query is optional
                ["orderId"] = orderId,
                ["from"] = from?.ToString("o"),
                ["to"] = to?.ToString("o"),
                ["paymentStatus"] = paymentStatus,
                ["tabStatus"] = tabStatus
            };

            var filtered = q
                .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            var uri = QueryHelpers.AddQueryString("customer/orders/search", filtered);

            var resp = await _http.GetAsync(uri);
            if (!resp.IsSuccessStatusCode)
                return Enumerable.Empty<Order>();

            var res = await resp.Content.ReadFromJsonAsync<IEnumerable<Order>>();
            return res ?? Enumerable.Empty<Order>();
        }

        public async Task<Order?> GetOrderDetailAsync(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId)) return null;
            return await GetOrNullAsync<Order>($"customer/orders/{Uri.EscapeDataString(orderId)}");
        }

        public async Task CancelOrderAsync(string orderId, string cancelReason)
        {
            var response = await _http.PutAsJsonAsync(
                $"customer/orders/{Uri.EscapeDataString(orderId)}/cancel",
                cancelReason
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Cancel failed: {error}");
            }
        }

        public async Task<bool> HasCustomerPurchasedProductAsync(string customerId, string productId)
        {
            // API reads customerId from claims; supply productId as query parameter
            var resp = await _http.GetAsync($"customer/orders/has-purchased?productId={Uri.EscapeDataString(productId)}");
            if (!resp.IsSuccessStatusCode) return false;
            return await resp.Content.ReadFromJsonAsync<bool>();
        }

        public async Task<Order> CreateAsync(OrderCreateDto dto)
        {
            var resp = await _http.PostAsJsonAsync("customer/orders", dto);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<Order>() ?? throw new InvalidOperationException("Empty response from customer");
        }

        public async Task UpdatePaymentStatusAsync(string orderId, string paymentMethod, string paymentStatus, string? note)
        {
            var update = new 
            {
                PaymentMethod = paymentMethod,
                Status = paymentStatus,
                Note = note
            };
            var uri = $"customer/orders/{Uri.EscapeDataString(orderId)}/payment-status";
            var resp = await _http.PutAsJsonAsync(uri, update);
            resp.EnsureSuccessStatusCode();
        }

        public async Task<Order?> GetByIdAsync(string orderId)
        {
            return await GetOrderDetailAsync(orderId);
        }
    }
}
