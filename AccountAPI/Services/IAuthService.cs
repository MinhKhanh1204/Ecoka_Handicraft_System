using AccountAPI.DTOs;

namespace AccountAPI.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto> Login(LoginRequestDto request);

        Task RegisterCustomerAsync(RegisterCustomerRequestDto request);
        Task ChangePasswordAsync(string accountId, ChangePasswordDto request);

        Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request);

        Task ResetPasswordAsync(ResetPasswordRequestDto request);

		Task<ProfileResponseDto> GetProfileAsync(string accountId);

		Task UpdateProfileAsync(string accountId, UpdateProfileRequestDto request);

        Task<LoginResponseDto> LoginSocial(SocialLoginRequestDto request);
    }
}
