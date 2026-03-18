using System.Net.Http.Headers;

namespace FeedbackAPI.Services.Implements
{
    public class OrderService : IOrderService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderService(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> HasPurchasedAsync(string productId)
        {
            try
            {
                // Forward the JWT token from the current request
                var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
                
                if (string.IsNullOrEmpty(authHeader))
                {
                    return false;
                }

                var request = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:5000/customer/orders/has-purchased?productId={productId}");
                request.Headers.Add("Authorization", authHeader);

                var res = await _http.SendAsync(request);

                if (!res.IsSuccessStatusCode)
                    return false;

                var result = await res.Content.ReadFromJsonAsync<bool>();
                return result;
            }
            catch
            {
                return false;
            }
        }
    }
}
