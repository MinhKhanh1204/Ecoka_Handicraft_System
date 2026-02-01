using AccountAPI.DTOs;

namespace AccountAPI.Services
{
    public interface IAuthService
    {
        LoginResponseDto Login(LoginRequestDto request);
    }

}
