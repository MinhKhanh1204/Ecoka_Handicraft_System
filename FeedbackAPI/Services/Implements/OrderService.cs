using System.Net.Http.Headers;

namespace FeedbackAPI.Services.Implements
{
    public class OrderService : IOrderService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<OrderService> _logger;

        public OrderService(HttpClient http, IHttpContextAccessor httpContextAccessor, ILogger<OrderService> logger)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<bool> HasPurchasedAsync(string productId, string customerId)
        {
            try
            {
                var context = _httpContextAccessor.HttpContext;
                var authHeader = context?.Request.Headers["Authorization"].ToString();

                // If Authorization header missing, try cookie named AccessToken
                if (string.IsNullOrEmpty(authHeader))
                {
                    var cookieToken = context?.Request.Cookies["AccessToken"];
                    if (!string.IsNullOrEmpty(cookieToken))
                    {
                        authHeader = "Bearer " + cookieToken;
                        _logger.LogDebug("Using AccessToken cookie for Order API call");
                    }
                }

                if (string.IsNullOrEmpty(authHeader))
                {
                    _logger.LogWarning("No Authorization header or AccessToken cookie found when checking purchase for product {ProductId}", productId);
                    return false;
                }

                // Call Order API passing both productId and customerId as query
                var requestUri = $"api/customer/orders/has-purchased?productId={Uri.EscapeDataString(productId)}&customerId={Uri.EscapeDataString(customerId)}";
                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

                // Set Authorization header properly
                try
                {
                    if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        var token = authHeader.Substring("Bearer ".Length).Trim();
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }
                    else
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authHeader);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse authorization header when calling Order API");
                    request.Headers.Add("Authorization", authHeader);
                }

                var res = await _http.SendAsync(request);

                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Order API returned {Status} when checking purchase for product {ProductId}", res.StatusCode, productId);
                    return false;
                }

                var result = await res.Content.ReadFromJsonAsync<bool>();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while checking purchase status for product {ProductId}", productId);
                return false;
            }
        }
    }
}
