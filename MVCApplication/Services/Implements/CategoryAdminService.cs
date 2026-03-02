using System.Net.Http.Json;
using System.Text.Json;
using MVCApplication.Models;

namespace MVCApplication.Services.Implements
{
    public class CategoryAdminService : ICategoryAdminService
    {
        private readonly HttpClient _http;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public CategoryAdminService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<CategoryDto>> GetAllAsync()
        {
            var response = await _http.GetAsync("categories");
            if (!response.IsSuccessStatusCode) return new List<CategoryDto>();

            var apiResponse = await response.Content
                .ReadFromJsonAsync<ApiResponse<List<CategoryDto>>>(_jsonOptions);
            return apiResponse?.Data ?? new List<CategoryDto>();
        }

        public async Task<List<CategoryDto>> SearchAsync(string keyword)
        {
            var response = await _http.GetAsync($"categories/search?keyword={Uri.EscapeDataString(keyword)}");
            if (!response.IsSuccessStatusCode) return new List<CategoryDto>();

            var apiResponse = await response.Content
                .ReadFromJsonAsync<ApiResponse<List<CategoryDto>>>(_jsonOptions);
            return apiResponse?.Data ?? new List<CategoryDto>();
        }

        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            var response = await _http.GetAsync($"categories/{id}");
            if (!response.IsSuccessStatusCode) return null;

            var apiResponse = await response.Content
                .ReadFromJsonAsync<ApiResponse<CategoryDto>>(_jsonOptions);
            return apiResponse?.Data;
        }

        public async Task<CategoryDto?> CreateAsync(CategoryCreateDto dto)
        {
            var response = await _http.PostAsJsonAsync("categories", dto);
            if (!response.IsSuccessStatusCode) return null;

            var apiResponse = await response.Content
                .ReadFromJsonAsync<ApiResponse<CategoryDto>>(_jsonOptions);
            return apiResponse?.Data;
        }

        public async Task<bool> UpdateAsync(int id, CategoryUpdateDto dto)
        {
            var response = await _http.PutAsJsonAsync($"categories/{id}", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _http.DeleteAsync($"categories/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
