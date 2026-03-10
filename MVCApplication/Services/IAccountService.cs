using MVCApplication.Models;
using MVCApplication.Models.DTOs;

namespace MVCApplication.Services
{
    public interface IAccountService
    {
        Task<LoginResponseDto> LoginAsync(LoginViewModel model);
        Task<bool> RegisterAsync(RegisterViewModel model);
        Task<CustomFormatter.ApiResponse<object>> ChangePasswordAsync(ChangePasswordViewModel model);
    }
}
