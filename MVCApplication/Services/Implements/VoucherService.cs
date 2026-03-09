using MVCApplication.CustomFormatter;
using MVCApplication.Models.DTOs;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MVCApplication.Services;

namespace MVCApplication.Services.Implements
{
    public class VoucherService : IVoucherService
    {
        private readonly HttpClient _httpClient;

        public VoucherService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<VoucherDto>> GetAllVouchersAsync()
        {
            var response = await _httpClient.GetAsync("/vouchers");
            if (!response.IsSuccessStatusCode) return new List<VoucherDto>();

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<VoucherDto>>>();
            return result?.Data ?? new List<VoucherDto>();
        }

        public async Task<VoucherDto?> GetVoucherByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"/vouchers/{id}");
            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<VoucherDto>>();
            return result?.Data;
        }
    }
}
