using Microsoft.AspNetCore.WebUtilities;
using MVCApplication.Areas.Admin.DTOs;
using System.Net;
using System.Net.Http.Json;

namespace MVCApplication.Areas.Admin.Services.Implements
{
    public class CategoryService : ICategoryService
    {
        private readonly HttpClient _http;

        public CategoryService(HttpClient http)
        {
            _http = http;
        }

        private static readonly IReadOnlyList<ReadCategoryDto> EmptyList = Array.Empty<ReadCategoryDto>();

        // Hàm helper đơn giản (giống GetOrNullAsync trong OrderService)
        private async Task<T?> GetOrNullAsync<T>(string uri) where T : class
        {
            var resp = await _http.GetAsync(uri);
            if (resp.StatusCode == HttpStatusCode.NotFound)
                return null;

            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<T>();
        }

        public async Task<IReadOnlyList<ReadCategoryDto>> GetAllAsync()
        {
            var resp = await _http.GetAsync("/categories");

            if (!resp.IsSuccessStatusCode)
                return EmptyList;

            var data = await resp.Content.ReadFromJsonAsync<List<ReadCategoryDto>>();
            return data?.AsReadOnly() ?? EmptyList;
        }

        public async Task<IReadOnlyList<ReadCategoryDto>> SearchAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return EmptyList;

            var uri = QueryHelpers.AddQueryString("/categories/search", "keyword", keyword);
            var resp = await _http.GetAsync(uri);

            if (!resp.IsSuccessStatusCode)
                return EmptyList;

            var data = await resp.Content.ReadFromJsonAsync<List<ReadCategoryDto>>();
            return data?.AsReadOnly() ?? EmptyList;
        }

        public async Task<ReadCategoryDto?> GetByIdAsync(int id)
        {
            return await GetOrNullAsync<ReadCategoryDto>($"/categories/{id}");
        }

        public async Task<ReadCategoryDto?> CreateAsync(CategoryCreateDto dto)
        {
            var resp = await _http.PostAsJsonAsync("/categories", dto);

            if (!resp.IsSuccessStatusCode)
                return null;

            return await resp.Content.ReadFromJsonAsync<ReadCategoryDto>();
        }

        public async Task<bool> UpdateAsync(int id, CategoryUpdateDto dto)
        {
            var resp = await _http.PutAsJsonAsync($"/categories/{id}", dto);

            if (resp.StatusCode == HttpStatusCode.NotFound)
                return false;

            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var resp = await _http.DeleteAsync($"/categories/{id}");

            if (resp.StatusCode == HttpStatusCode.NotFound)
                return false;

            return resp.IsSuccessStatusCode;
        }
    }
}