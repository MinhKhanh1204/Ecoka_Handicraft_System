using System.Net.Http.Json;
using System.Text.Json;
using MVCApplication.Areas.Admin.DTOs;
using MVCApplication.Models;

namespace MVCApplication.Areas.Admin.Services.Implements
{
    public class StaffAdminService : IStaffAdminService
    {
        private readonly HttpClient _http;

        public StaffAdminService(HttpClient http)
        {
            _http = http;
        }

        public async Task<StaffPagedResult> GetStaffsAsync(string? keyword, string? role, bool? status, int page = 1, int pageSize = 10)
        {
            var queryParams = new List<string>
            {
                $"page={page}",
                $"pageSize={pageSize}"
            };

            if (!string.IsNullOrWhiteSpace(keyword))
                queryParams.Add($"keyword={Uri.EscapeDataString(keyword)}");
            if (!string.IsNullOrWhiteSpace(role))
                queryParams.Add($"role={Uri.EscapeDataString(role)}");
            if (status.HasValue)
                queryParams.Add($"status={status.Value}");

            var url = "/admin/staffs?" + string.Join("&", queryParams);

            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return new StaffPagedResult();

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<StaffPagedResult>>();
            return result?.Data ?? new StaffPagedResult();
        }

        public async Task<StaffDetailViewModel?> GetStaffDetailAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            var response = await _http.GetAsync($"/admin/staffs/{Uri.EscapeDataString(id)}");
            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<StaffDetailViewModel>>();
            return result?.Data;
        }

        public async Task<(bool Success, string? ErrorMessage)> CreateStaffAsync(CreateStaffViewModel model)
        {
            var payload = new
            {
                fullName = model.FullName,
                email = model.Email,
                phone = model.Phone,
                password = model.Password,
                roleID = model.RoleID,
                address = model.Address,
                gender = model.Gender,
                citizenId = model.CitizenId,
                dateOfBirth = model.DateOfBirth?.ToString("yyyy-MM-dd")
            };

            var response = await _http.PostAsJsonAsync("/admin/staffs", payload);

            if (response.IsSuccessStatusCode)
                return (true, null);

            // Try to read error message from API response
            try
            {
                var errorJson = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(errorJson);
                var message = doc.RootElement.GetProperty("message").GetString();
                return (false, message ?? "Failed to create staff");
            }
            catch
            {
                return (false, "Failed to create staff");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateStaffAsync(EditStaffViewModel model)
        {
            // API expects JSON body ([FromBody] UpdateStaffDto), not multipart/form-data.
            var payload = new
            {
                staffId = model.StaffId,
                fullName = model.FullName,
                email = model.Email,
                phone = model.Phone ?? "",
                address = model.Address ?? "",
                gender = model.Gender ?? "",
                citizenId = model.CitizenId ?? "",
                dateOfBirth = model.DateOfBirth?.ToString("yyyy-MM-dd"),
                status = model.Status,
                avatar = model.Avatar
            };

            var response = await _http.PutAsJsonAsync($"/admin/staffs/{Uri.EscapeDataString(model.StaffId)}", payload);
            if (response.IsSuccessStatusCode)
                return (true, null);

            try
            {
                var errorJson = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(errorJson);
                var message = doc.RootElement.TryGetProperty("message", out var msgEl) ? msgEl.GetString() : null;
                return (false, message ?? $"Failed to update staff (HTTP {(int)response.StatusCode})");
            }
            catch
            {
                return (false, $"Failed to update staff (HTTP {(int)response.StatusCode})");
            }
        }

        public async Task<bool> DeleteStaffAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            var response = await _http.DeleteAsync($"/admin/staffs/{Uri.EscapeDataString(id)}");
            return response.IsSuccessStatusCode;
        }
    }
}
