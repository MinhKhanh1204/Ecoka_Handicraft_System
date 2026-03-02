using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MVCApplication.Models;

namespace MVCApplication.Services.Implements
{
    public class CustomerService : ICustomerService
    {
        private readonly HttpClient _http;

        public CustomerService(HttpClient http)
        {
            _http = http;
        }

        public async Task<IEnumerable<CustomerViewModel>> GetAllAsync()
        {
            var response = await _http.GetAsync("/admin/customers");
            if (!response.IsSuccessStatusCode)
                return Enumerable.Empty<CustomerViewModel>();

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<CustomerViewModel>>>();
            return result?.Data ?? Enumerable.Empty<CustomerViewModel>();
        }

        public async Task<IEnumerable<CustomerViewModel>> SearchAsync(string? keyword, string? status)
        {
            var queryParams = new Dictionary<string, string?>();
            
            if (!string.IsNullOrWhiteSpace(keyword))
                queryParams["keyword"] = keyword;
            if (!string.IsNullOrWhiteSpace(status))
                queryParams["status"] = status;

            var url = "/admin/customers/search";
            if (queryParams.Count > 0)
            {
                var queryString = string.Join("&", queryParams.Select(kv => $"{kv.Key}={kv.Value}"));
                url += "?" + queryString;
            }

            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return Enumerable.Empty<CustomerViewModel>();

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<CustomerViewModel>>>();
            return result?.Data ?? Enumerable.Empty<CustomerViewModel>();
        }

        public async Task<CustomerViewModel?> GetByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            var response = await _http.GetAsync($"/admin/customers/{Uri.EscapeDataString(id)}");
            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<CustomerViewModel>>();
            return result?.Data;
        }

        public async Task<bool> UpdateStatusAsync(string id, string status)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            // Validate status
            if (string.IsNullOrWhiteSpace(status) || (status != "Active" && status != "Inactive"))
                return false;

            var statusDto = new CustomerStatusModel { Status = status };
            var response = await _http.PutAsJsonAsync($"/admin/customers/{Uri.EscapeDataString(id)}/status", statusDto);
            return response.IsSuccessStatusCode;
        }
    }
}
