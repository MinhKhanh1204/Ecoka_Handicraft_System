using Microsoft.AspNetCore.WebUtilities;
using MVCApplication.Areas.Admin.DTOs;
using MVCApplication.CustomFormatter;
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
        private const string BasePath = "/admin/categories";

        private async Task<ApiResponse<T>?> ReadApiResponseAsync<T>(HttpResponseMessage resp)
        {
            if (resp.Content == null)
                return null;

            return await resp.Content.ReadFromJsonAsync<ApiResponse<T>>();
        }

        public async Task<IReadOnlyList<ReadCategoryDto>> GetAllAsync()
        {
            var resp = await _http.GetAsync(BasePath);
            if (!resp.IsSuccessStatusCode)
                return EmptyList;

            var api = await ReadApiResponseAsync<List<ReadCategoryDto>>(resp);
            var data = api?.Data ?? new List<ReadCategoryDto>();
            return data.AsReadOnly();
        }

        public async Task<IReadOnlyList<ReadCategoryDto>> SearchAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return EmptyList;

            var uri = QueryHelpers.AddQueryString($"{BasePath}/search", "keyword", keyword);
            var resp = await _http.GetAsync(uri);
            if (!resp.IsSuccessStatusCode)
                return EmptyList;

            var api = await ReadApiResponseAsync<List<ReadCategoryDto>>(resp);
            var data = api?.Data ?? new List<ReadCategoryDto>();
            return data.AsReadOnly();
        }

        public async Task<ReadCategoryDto?> GetByIdAsync(int id)
        {
            var resp = await _http.GetAsync($"{BasePath}/{id}");
            if (resp.StatusCode == HttpStatusCode.NotFound)
                return null;

            resp.EnsureSuccessStatusCode();
            var api = await ReadApiResponseAsync<ReadCategoryDto>(resp);
            return api?.Data;
        }

        public async Task<ReadCategoryDto?> CreateAsync(CategoryCreateDto dto)
        {
            var resp = await _http.PostAsJsonAsync(BasePath, dto);
            if (!resp.IsSuccessStatusCode)
                return null;

            var api = await ReadApiResponseAsync<ReadCategoryDto>(resp);
            return api?.Data;
        }

        public async Task<bool> UpdateAsync(int id, CategoryUpdateDto dto)
        {
            var resp = await _http.PutAsJsonAsync($"{BasePath}/{id}", dto);
            if (resp.StatusCode == HttpStatusCode.NotFound)
                return false;

            if (!resp.IsSuccessStatusCode)
                return false;

            var api = await ReadApiResponseAsync<bool>(resp);
            return api?.Data == true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var resp = await _http.DeleteAsync($"{BasePath}/{id}");
            if (resp.StatusCode == HttpStatusCode.NotFound)
                return false;

            if (!resp.IsSuccessStatusCode)
                return false;

            var api = await ReadApiResponseAsync<bool>(resp);
            return api?.Data == true;
        }
    }
}
