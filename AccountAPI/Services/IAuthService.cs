using AccountAPI.DTOs;

namespace AccountAPI.Services
{
    public interface IAuthService
    {
        LoginResponseDto Login(LoginRequestDto request);

        Task RegisterCustomerAsync(RegisterCustomerRequestDto request);

        Task ForgotPasswordAsync(ForgotPasswordRequestDto request);

        Task ResetPasswordAsync(ResetPasswordRequestDto request);
    }
}
