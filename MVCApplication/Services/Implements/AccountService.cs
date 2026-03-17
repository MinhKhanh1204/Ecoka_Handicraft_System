using MVCApplication.Models;
using MVCApplication.Models.DTOs;
using System.Text;
using System.Text.Json;

namespace MVCApplication.Services.Implements
{
    public class AccountService : IAccountService
    {
        private readonly HttpClient _httpClient;

        public AccountService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CustomFormatter.ApiResponse<LoginResponseDto>> LoginAsync(LoginViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("auth/login", model);

            var result = await response.Content.ReadFromJsonAsync<CustomFormatter.ApiResponse<LoginResponseDto>>();

            return result!;
        }

        public async Task<ApiResponse<object>> RegisterAsync(RegisterViewModel model)
        {
            using var formData = new MultipartFormDataContent();

            formData.Add(new StringContent(model.Username), "Username");
            formData.Add(new StringContent(model.Email), "Email");
            formData.Add(new StringContent(model.Password), "Password");
            formData.Add(new StringContent(model.FullName), "FullName");

            if (model.DateOfBirth.HasValue)
                formData.Add(new StringContent(model.DateOfBirth.Value.ToString("yyyy-MM-dd")), "DateOfBirth");

            if (!string.IsNullOrEmpty(model.Gender))
                formData.Add(new StringContent(model.Gender), "Gender");

            if (!string.IsNullOrEmpty(model.Phone))
                formData.Add(new StringContent(model.Phone), "Phone");

            if (!string.IsNullOrEmpty(model.Address))
                formData.Add(new StringContent(model.Address), "Address");

            // ===== Avatar =====
            if (model.Avatar != null && model.Avatar.Length > 0)
            {
                var streamContent = new StreamContent(model.Avatar.OpenReadStream());
                streamContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(model.Avatar.ContentType);

                formData.Add(streamContent, "Avatar", model.Avatar.FileName);
            }
            var response = await _httpClient.PostAsync("auth/register-customer", formData);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            return result!;
        }

        public async Task<CustomFormatter.ApiResponse<object>> ChangePasswordAsync(ChangePasswordViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("auth/change-password", model);
            
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return CustomFormatter.ApiResponse<object>.Fail("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", 401);
            }

            if (!response.IsSuccessStatusCode)
            {
                try 
                {
                    var errorResult = await response.Content.ReadFromJsonAsync<CustomFormatter.ApiResponse<object>>();
                    return errorResult ?? CustomFormatter.ApiResponse<object>.Fail("Đã có lỗi xảy ra", (int)response.StatusCode);
                }
                catch
                {
                    return CustomFormatter.ApiResponse<object>.Fail($"Lỗi hệ thống: {response.StatusCode}", (int)response.StatusCode);
                }
            }
            
            var result = await response.Content.ReadFromJsonAsync<CustomFormatter.ApiResponse<object>>();
            
            return result ?? CustomFormatter.ApiResponse<object>.Fail("Đã có lỗi xảy ra", (int)response.StatusCode);
        }
        public async Task<bool> ForgotPasswordAsync(ForgotPasswordViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("auth/forgot-password", new { model.Email });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("auth/reset-password", new
            {
                model.Token,
                model.NewPassword
            });
            return response.IsSuccessStatusCode;
        }
    }
}
