using System.Net.Http.Json;
using MVCApplication.Areas.Admin.DTOs;

namespace MVCApplication.Areas.Admin.Services.Implements
{
    public class AdminOrderService : IAdminOrderService
    {
        private readonly HttpClient _http;

        public AdminOrderService(HttpClient http)
        {
            _http = http;
        }

        public async Task<IEnumerable<RevenueByMonthDto>> GetRevenueByYearAsync(int year)
        {
            var response = await _http.GetAsync($"admin/orders/revenue?year={year}");

            if (!response.IsSuccessStatusCode)
                return Enumerable.Empty<RevenueByMonthDto>();

            return await response.Content.ReadFromJsonAsync<IEnumerable<RevenueByMonthDto>>()
                   ?? Enumerable.Empty<RevenueByMonthDto>();
        }
    }
}
