using System.Net.Http.Json;
using System.Text.Json;
using MVCApplication.Models;

namespace MVCApplication.Services.Implements
{
    public class ProductAdminService : IProductAdminService
    {
        private readonly HttpClient _http;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ProductAdminService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<ProductListDto>> GetAllAsync()
        {
            // ProductAdminController returns plain list (not wrapped in ApiResponse)
            var result = await _http.GetFromJsonAsync<List<ProductListDto>>("admin/products", _jsonOptions);
            return result ?? new List<ProductListDto>();
        }

        public async Task<List<ProductListDto>> SearchAsync(string keyword)
        {
            // ProductAdminController returns plain list (not wrapped in ApiResponse)
            var result = await _http.GetFromJsonAsync<List<ProductListDto>>(
                $"admin/products/search?keyword={Uri.EscapeDataString(keyword)}", _jsonOptions);
            return result ?? new List<ProductListDto>();
        }

        public async Task<ProductDetailDto?> GetByIdAsync(string id)
        {
            var response = await _http.GetAsync($"admin/products/{Uri.EscapeDataString(id)}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<ProductDetailDto>(_jsonOptions);
        }

        public async Task<bool> CreateAsync(ProductCreateDto dto)
        {
            var response = await _http.PostAsJsonAsync("admin/products", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(string id, ProductUpdateDto dto)
        {
            var response = await _http.PutAsJsonAsync($"admin/products/{Uri.EscapeDataString(id)}", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ApproveAsync(string id)
        {
            var response = await _http.PutAsync($"admin/products/approve/{Uri.EscapeDataString(id)}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RejectAsync(string id)
        {
            var response = await _http.PutAsync($"admin/products/reject/{Uri.EscapeDataString(id)}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _http.DeleteAsync($"admin/products/{Uri.EscapeDataString(id)}");
            return response.IsSuccessStatusCode;
        }
    }
}
