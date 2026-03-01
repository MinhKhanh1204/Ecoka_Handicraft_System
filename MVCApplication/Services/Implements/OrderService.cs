using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using MVCApplication.Models;

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
            //if (string.IsNullOrWhiteSpace(customerId))
            //    return Enumerable.Empty<Order>();

            var response = await _http.GetAsync($"/customer/orders");

            if (!response.IsSuccessStatusCode)
                return Enumerable.Empty<Order>();

            return await response.Content.ReadFromJsonAsync<IEnumerable<Order>>()
                   ?? Enumerable.Empty<Order>();
        }

        public async Task<IEnumerable<Order>> SearchOrdersAsync(string customerId, string? orderId, DateTime? from, DateTime? to, string? paymentStatus, string? tabStatus)
        {
            var q = new Dictionary<string, string?>
            {
                ["customerId"] = customerId,
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
            var res = await _http.GetFromJsonAsync<IEnumerable<Order>>(uri);
            return res ?? Enumerable.Empty<Order>();
        }

        public async Task<Order?> GetOrderDetailAsync(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId)) return null;
            return await GetOrNullAsync<Order>($"customer/orders/{Uri.EscapeDataString(orderId)}");
        }

        public async Task<bool> CancelOrderAsync(string orderId, string cancelReason)
        {
            var uri = QueryHelpers.AddQueryString($"customer/orders/{Uri.EscapeDataString(orderId)}", new Dictionary<string, string?> { ["reason"] = cancelReason });
            var resp = await _http.DeleteAsync(uri);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> HasCustomerPurchasedProductAsync(string customerId, string productId)
        {
            var resp = await _http.GetAsync($"customer/orders/{Uri.EscapeDataString(customerId)}/purchased/{Uri.EscapeDataString(productId)}");
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
            var update = new OrderUpdateDto
            {
                PaymentMethod = paymentMethod,
                PaymentStatus = paymentStatus,
                Note = note
            };
            var uri = $"customer/orders/{Uri.EscapeDataString(orderId)}";
            var resp = await _http.PutAsJsonAsync(uri, update);
            resp.EnsureSuccessStatusCode();
        }

        public async Task<Order?> GetByIdAsync(string orderId)
        {
            return await GetOrderDetailAsync(orderId);
        }
    }
}
