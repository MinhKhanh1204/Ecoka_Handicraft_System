using MVCApplication.CustomFormatter;
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

        public async Task<LoginResponseDto> LoginAsync(LoginViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("auth/login", model);

            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content.ReadFromJsonAsync<CustomFormatter.ApiResponse<LoginResponseDto>>();

            return result?.Data;
        }

        public async Task<bool> RegisterAsync(RegisterViewModel model)
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

            return response.IsSuccessStatusCode;
        }
    }
}
