using AccountAPI.DTOs;

namespace AccountAPI.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto> Login(LoginRequestDto request);

        Task RegisterCustomerAsync(RegisterCustomerRequestDto request);
        Task ChangePasswordAsync(string accountId, ChangePasswordDto request);

        Task ForgotPasswordAsync(ForgotPasswordRequestDto request);

        Task ResetPasswordAsync(ResetPasswordRequestDto request);
    }
}
