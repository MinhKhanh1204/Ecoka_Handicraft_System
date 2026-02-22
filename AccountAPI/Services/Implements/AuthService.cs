using AccountAPI.DTOs;
using AccountAPI.Exceptions;
using AccountAPI.Helpers;
using AccountAPI.Mappers;
using AccountAPI.Models;
using AccountAPI.Repositories;

namespace AccountAPI.Services.Implements
{
    public class AuthService : IAuthService
    {
        private readonly IAccountRepository _repo;
        private readonly JwtTokenHelper _jwt;
        private readonly IAccountMapper _mapper;
        private readonly ICloudinaryService _cloudinaryService;

        public AuthService(
            IAccountRepository repo,
            JwtTokenHelper jwt,
            IAccountMapper mapper,
            ICloudinaryService cloudinaryService)
        {
            _repo = repo;
            _jwt = jwt;
            _mapper = mapper;
            _cloudinaryService = cloudinaryService;
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

        public async Task RegisterCustomerAsync(RegisterCustomerRequestDto request)
        {
            if (await _repo.UsernameExistsAsync(request.Username))
                throw new BadRequestException("Username already exists");

            if (await _repo.EmailExistsAsync(request.Email))
                throw new BadRequestException("Email already exists");

            using var transaction = await _repo.BeginTransactionAsync();

            try
            {
                var accountId = await _repo.GenerateAccountIdAsync();

                string? avatarUrl = null;

                if (request.Avatar != null)
                {
                    avatarUrl = await _cloudinaryService.UploadImageAsync(request.Avatar);
                }

                var account = new Account
                {
                    AccountID = accountId,
                    Username = request.Username,
                    Email = request.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Avatar = avatarUrl,
                    CreatedAt = DateTime.UtcNow,
                    Status = "Active"
                };

                await _repo.AddAccountAsync(account);

                var customer = new Customer
                {
                    CustomerID = accountId,
                    FullName = request.FullName,
                    DateOfBirth = request.DateOfBirth,
                    Gender = request.Gender,
                    Phone = request.Phone,
                    Address = request.Address,
                    Status = "Active"
                };

                await _repo.AddCustomerAsync(customer);

                var role = await _repo.GetCustomerRoleAsync();
                if (role == null)
                    throw new BadRequestException("Customer role not found");

                await _repo.AddUserRoleAsync(new UserRole
                {
                    AccountID = accountId,
                    RoleID = role.RoleID,
                    Status = "Active"
                });

                await _repo.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    }

}
