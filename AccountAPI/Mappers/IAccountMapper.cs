using AccountAPI.DTOs;
using AccountAPI.Models;

namespace AccountAPI.Mappers
{
    public interface IAccountMapper
    {
        LoginResponseDto ToLoginResponse(string token);
    }
}
