using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FeedbackAPI.Services;

namespace FeedbackAPI.Services.Implements
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
            if (string.IsNullOrWhiteSpace(customerId))
                return null;

            try
            {
                // Try several likely endpoints; adjust to your real account API routes if needed
                var candidates = new[]
                {
                    $"auth/profile/{Uri.EscapeDataString(customerId)}",
                    $"auth/accounts/{Uri.EscapeDataString(customerId)}",
                    $"accounts/{Uri.EscapeDataString(customerId)}",
                    $"auth/get-username/{Uri.EscapeDataString(customerId)}"
                };

                foreach (var path in candidates)
                {
                    try
                    {
                        var resp = await _http.GetAsync(path);
                        if (!resp.IsSuccessStatusCode)
                            continue;

                        // Read JSON and attempt to extract username in several shapes
                        var json = await resp.Content.ReadFromJsonAsync<JsonElement?>();

                        if (json.HasValue)
                        {
                            var root = json.Value;

                            if (root.ValueKind == JsonValueKind.Object)
                            {
                                if (root.TryGetProperty("username", out var usernameProp) && usernameProp.ValueKind == JsonValueKind.String)
                                    return usernameProp.GetString();

                                if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object)
                                {
                                    if (data.TryGetProperty("username", out var u2) && u2.ValueKind == JsonValueKind.String)
                                        return u2.GetString();
                                }
                            }
                            else if (root.ValueKind == JsonValueKind.String)
                            {
                                return root.GetString();
                            }
                        }
                    }
                    catch
                    {
                        // ignore and try next candidate
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}