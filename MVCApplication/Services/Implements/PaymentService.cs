using MVCApplication.Models.DTOs;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace MVCApplication.Services.Implements
{
    public class PaymentService : IPaymentService
    {
        private readonly HttpClient _httpClient;

        public PaymentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string?> GetVnPayUrlAsync(string orderId, decimal amount, string orderInfo, string returnUrl)
        {
            var request = new { OrderId = orderId, Amount = amount, OrderInfo = orderInfo, ReturnUrl = returnUrl };
            var response = await _httpClient.PostAsJsonAsync("payment/create-vnpay-url", request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<PaymentResult>();
                return result?.PaymentUrl;
            }
            return null;
        }

        public async Task<string?> GetMomoUrlAsync(string orderId, decimal amount, string orderInfo, string returnUrl)
        {
            var request = new { OrderId = orderId, Amount = amount, OrderInfo = orderInfo, ReturnUrl = returnUrl };
            var response = await _httpClient.PostAsJsonAsync("payment/create-momo-url", request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<PaymentResult>();
                return result?.PaymentUrl;
            }
            return null;
        }

        private class PaymentResult
        {
            public bool Success { get; set; }
            public string? PaymentUrl { get; set; }
        }
    }
}
