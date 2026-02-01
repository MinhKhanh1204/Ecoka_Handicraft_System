using AccountAPI.DTOs;
using AccountAPI.Models;

namespace AccountAPI.Mappers.Implements
{
    public class AccountMapper : IAccountMapper
    {
        public LoginResponseDto ToLoginResponse(string token)
        {
            return new LoginResponseDto
            {
                AccessToken = token
            };
        }
    }
}
