using System.Text.Json;

namespace FeedbackAPI.Services
{
    public class AccountService : IAccountService
    {
        private readonly HttpClient _http;

        public AccountService(HttpClient http)
        {
            _http = http;
        }

        public async Task<string?> GetUsernameAsync(string customerId)
        {
            try
            {
                var res = await _http.GetAsync(
                    $"https://localhost:5000/admin/customers/{customerId}");

                if (!res.IsSuccessStatusCode)
                    return customerId;

                var json = await res.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(json);

                var username = doc
                    .RootElement
                    .GetProperty("data")
                    .GetProperty("username")
                    .GetString();

                return username ?? customerId;
            }
            catch
            {
                return customerId;
            }
        }

    }
}