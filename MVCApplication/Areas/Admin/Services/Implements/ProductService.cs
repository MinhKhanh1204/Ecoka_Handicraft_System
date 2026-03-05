using Microsoft.AspNetCore.WebUtilities;
using MVCApplication.Areas.Admin.DTOs;
using MVCApplication.CustomFormatter;
using System.Net;
using System.Net.Http.Json;

namespace MVCApplication.Areas.Admin.Services.Implements
{
    public class ProductService : IProductAdminService
    {
        private readonly HttpClient _http;

        public ProductService(HttpClient http)
        {
            _http = http;
        }

        private const string BasePath = "/admin/products";

        private async Task<ApiResponse<T>?> ReadApiResponseAsync<T>(HttpResponseMessage resp)
        {
            if (resp.Content == null)
                return null;

            return await resp.Content.ReadFromJsonAsync<ApiResponse<T>>();
        }

        public async Task<PagedResult<ReadProductDto>> GetPagedAsync(
            string? keyword,
            string? status,
            string? userRole,
            int pageNumber,
            int pageSize)
        {
            var queryParams = new Dictionary<string, string?>
            {
                ["pageNumber"] = pageNumber.ToString(),
                ["pageSize"] = pageSize.ToString()
            };

            if (!string.IsNullOrWhiteSpace(keyword))
                queryParams["keyword"] = keyword;

            if (!string.IsNullOrWhiteSpace(status))
                queryParams["status"] = status;

            if (!string.IsNullOrWhiteSpace(userRole))
                queryParams["userRole"] = userRole;

            var uri = QueryHelpers.AddQueryString(BasePath, queryParams);

            var resp = await _http.GetAsync(uri);
            resp.EnsureSuccessStatusCode();  

            var api = await ReadApiResponseAsync<PagedResult<ReadProductDto>>(resp);

            if (api == null || !api.Success || api.Data == null)
                return new PagedResult<ReadProductDto>
                {
                    Items = new List<ReadProductDto>(),
                    TotalCount = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

            return api.Data;
        }

        public async Task<ProductDetailDto?> GetByIdAsync(string id)
        {
            var resp = await _http.GetAsync($"{BasePath}/{id}");
            if (resp.StatusCode == HttpStatusCode.NotFound)
                return null;

            resp.EnsureSuccessStatusCode();
            var api = await ReadApiResponseAsync<ProductDetailDto>(resp);
            return api?.Data;
        }

        public async Task<bool> CreateAsync(CreateProductDto dto)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(dto.CategoryID.ToString()), nameof(dto.CategoryID));
            content.Add(new StringContent(dto.ProductName), nameof(dto.ProductName));
            if (dto.Description != null) content.Add(new StringContent(dto.Description), nameof(dto.Description));
            if (dto.Material != null) content.Add(new StringContent(dto.Material), nameof(dto.Material));
            content.Add(new StringContent(dto.Price.ToString()), nameof(dto.Price));
            content.Add(new StringContent(dto.Discount.ToString()), nameof(dto.Discount));
            content.Add(new StringContent(dto.StockQuantity.ToString()), nameof(dto.StockQuantity));

            for (int i = 0; i < dto.Images.Count; i++)
            {
                var img = dto.Images[i];
                if (!string.IsNullOrWhiteSpace(img.ImageUrl))
                {
                    content.Add(new StringContent(img.ImageUrl), $"Images[{i}].ImageUrl");
                }
                if (img.ImageFile != null)
                {
                    var fileContent = new StreamContent(img.ImageFile.OpenReadStream());
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(img.ImageFile.ContentType);
                    content.Add(fileContent, $"Images[{i}].ImageFile", img.ImageFile.FileName);
                }
                content.Add(new StringContent(img.IsMain.ToString().ToLower()), $"Images[{i}].IsMain");
            }

            var resp = await _http.PostAsync(BasePath, content);
            if (!resp.IsSuccessStatusCode)
                return false;

            var api = await ReadApiResponseAsync<object>(resp);
            return api?.Success == true;
        }

        public async Task<bool> UpdateAsync(string id, UpdateProductDto dto)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(dto.CategoryID.ToString()), nameof(dto.CategoryID));
            content.Add(new StringContent(dto.ProductName), nameof(dto.ProductName));
            if (dto.Description != null) content.Add(new StringContent(dto.Description), nameof(dto.Description));
            if (dto.Material != null) content.Add(new StringContent(dto.Material), nameof(dto.Material));
            content.Add(new StringContent(dto.Price.ToString()), nameof(dto.Price));
            content.Add(new StringContent(dto.Discount.ToString()), nameof(dto.Discount));
            content.Add(new StringContent(dto.StockQuantity.ToString()), nameof(dto.StockQuantity));
            content.Add(new StringContent(dto.Status), nameof(dto.Status));

            for (int i = 0; i < dto.Images.Count; i++)
            {
                var img = dto.Images[i];
                if (!string.IsNullOrWhiteSpace(img.ImageUrl))
                {
                    content.Add(new StringContent(img.ImageUrl), $"Images[{i}].ImageUrl");
                }
                if (img.ImageFile != null)
                {
                    var fileContent = new StreamContent(img.ImageFile.OpenReadStream());
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(img.ImageFile.ContentType);
                    content.Add(fileContent, $"Images[{i}].ImageFile", img.ImageFile.FileName);
                }
                content.Add(new StringContent(img.IsMain.ToString().ToLower()), $"Images[{i}].IsMain");
            }

            var resp = await _http.PutAsync($"{BasePath}/{id}", content);
            if (!resp.IsSuccessStatusCode)
                return false;

            var api = await ReadApiResponseAsync<object>(resp);
            return api?.Success == true;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var resp = await _http.DeleteAsync($"{BasePath}/{id}");
            if (!resp.IsSuccessStatusCode)
                return false;

            var api = await ReadApiResponseAsync<object>(resp);
            return api?.Success == true;
        }

        public async Task<bool> ApproveAsync(string id)
        {
            var resp = await _http.PutAsync($"{BasePath}/approve/{id}", null);
            if (!resp.IsSuccessStatusCode)
                return false;

            var api = await ReadApiResponseAsync<object>(resp);
            return api?.Success == true;
        }

        public async Task<bool> RejectAsync(string id)
        {
            var resp = await _http.PutAsync($"{BasePath}/reject/{id}", null);
            if (!resp.IsSuccessStatusCode)
                return false;

            var api = await ReadApiResponseAsync<object>(resp);
            return api?.Success == true;
        }
    }
}
