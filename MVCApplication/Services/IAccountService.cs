using MVCApplication.Models;
using MVCApplication.Models.DTOs;

namespace MVCApplication.Services
{
    public interface IAccountService
    {
        Task<CustomFormatter.ApiResponse<LoginResponseDto>> LoginAsync(LoginViewModel model);
        Task<CustomFormatter.ApiResponse<object>> RegisterAsync(RegisterViewModel model);
        Task<CustomFormatter.ApiResponse<object>> ChangePasswordAsync(ChangePasswordViewModel model);
        Task<bool> ForgotPasswordAsync(ForgotPasswordViewModel model);
        Task<bool> ResetPasswordAsync(ResetPasswordViewModel model);
        Task<ProfileViewModel> GetProfileAsync();
        Task<CustomFormatter.ApiResponse<object>> UpdateProfileAsync(ProfileViewModel model, IFormFile? avatar);
    }
}
