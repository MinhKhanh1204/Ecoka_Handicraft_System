using Microsoft.AspNetCore.WebUtilities;
using MVCApplication.Areas.Admin.DTOs;
using MVCApplication.CustomFormatter;
using System.Net;
using System.Net.Http.Json;

namespace MVCApplication.Areas.Admin.Services.Implements
{
    public class VoucherAdminService : IVoucherAdminService
    {
        private readonly HttpClient _http;
        private const string BasePath = "/admin/vouchers";

        public VoucherAdminService(HttpClient http)
        {
            _http = http;
        }

        private static async Task<ApiResponse<T>?> ReadApiResponseAsync<T>(HttpResponseMessage resp)
        {
            if (resp.Content == null) return null;
            return await resp.Content.ReadFromJsonAsync<ApiResponse<T>>();
        }

        public async Task<PagedResult<VoucherListDto>> GetPagedAsync(
            string? keyword,
            string? status,
            string? sortBy,
            int pageNumber,
            int pageSize)
        {
            var queryParams = new Dictionary<string, string?>
            {
                ["pageNumber"] = pageNumber.ToString(),
                ["pageSize"] = pageSize.ToString()
            };
            if (!string.IsNullOrWhiteSpace(keyword)) queryParams["keyword"] = keyword;
            if (!string.IsNullOrWhiteSpace(status)) queryParams["status"] = status;
            if (!string.IsNullOrWhiteSpace(sortBy)) queryParams["sortBy"] = sortBy;

            var uri = QueryHelpers.AddQueryString(BasePath, queryParams);
            var resp = await _http.GetAsync(uri);

            if (!resp.IsSuccessStatusCode)
                return new PagedResult<VoucherListDto> { Items = new List<VoucherListDto>(), TotalCount = 0, PageNumber = pageNumber, PageSize = pageSize };

            var api = await ReadApiResponseAsync<PagedResult<VoucherListDto>>(resp);
            return api?.Data ?? new PagedResult<VoucherListDto> { Items = new List<VoucherListDto>(), TotalCount = 0, PageNumber = pageNumber, PageSize = pageSize };
        }

        public async Task<VoucherDetailDto?> GetByIdAsync(int id)
        {
            var resp = await _http.GetAsync($"{BasePath}/{id}");
            if (resp.StatusCode == HttpStatusCode.NotFound) return null;
            resp.EnsureSuccessStatusCode();
            var api = await ReadApiResponseAsync<VoucherDetailDto>(resp);
            return api?.Data;
        }

        public async Task<bool> CreateAsync(CreateVoucherDto dto)
        {
            var resp = await _http.PostAsJsonAsync(BasePath, dto);
            if (!resp.IsSuccessStatusCode) return false;
            var api = await ReadApiResponseAsync<object>(resp);
            return api?.Success == true;
        }

        public async Task<bool> UpdateAsync(int id, UpdateVoucherDto dto)
        {
            var resp = await _http.PutAsJsonAsync($"{BasePath}/{id}", dto);
            if (!resp.IsSuccessStatusCode) return false;
            var api = await ReadApiResponseAsync<object>(resp);
            return api?.Success == true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var resp = await _http.DeleteAsync($"{BasePath}/{id}");
            if (!resp.IsSuccessStatusCode) return false;
            var api = await ReadApiResponseAsync<object>(resp);
            return api?.Success == true;
        }
    }
}
