using AccountAPI.DTOs;
using AccountAPI.Exceptions;
using AccountAPI.Helpers;
using AccountAPI.Mappers;
using AccountAPI.Repositories;

namespace AccountAPI.Services.Implements
{
    public class AuthService : IAuthService
    {
        private readonly IAccountRepository _repo;
        private readonly JwtTokenHelper _jwt;
        private readonly IAccountMapper _mapper;

        public AuthService(
            IAccountRepository repo,
            JwtTokenHelper jwt,
            IAccountMapper mapper)
        {
            _repo = repo;
            _jwt = jwt;
            _mapper = mapper;
        }

        public LoginResponseDto Login(LoginRequestDto request)
        {
            var account = _repo.GetByUsername(request.Username);
            if (account == null)
                throw new UnauthorizedException("Invalid username or password");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, account.Password))
                throw new UnauthorizedException("Invalid username or password");

            if (account.Status != "Active")
                throw new ForbiddenException("Account is inactive");

            var token = _jwt.GenerateToken(account);

            return _mapper.ToLoginResponse(token);
        }
    }

}
