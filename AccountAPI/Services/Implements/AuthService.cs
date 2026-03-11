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
        private readonly IEmailService _emailService;
        private readonly PasswordResetSettings _resetSettings;

        public AuthService(
            IAccountRepository repo,
            JwtTokenHelper jwt,
            IAccountMapper mapper,
            ICloudinaryService cloudinaryService,
            IEmailService emailService,
            Microsoft.Extensions.Options.IOptions<PasswordResetSettings> resetSettings)
        {
            _repo = repo;
            _jwt = jwt;
            _mapper = mapper;
            _cloudinaryService = cloudinaryService;
            _emailService = emailService;
            _resetSettings = resetSettings.Value;
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

        public async Task ChangePasswordAsync(string accountId, ChangePasswordDto request)
        {
            var account = await _repo.GetByIdAsync(accountId);
            if (account == null)
                throw new NotFoundException("Account not found");

            if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, account.Password))
                throw new BadRequestException("Old password is incorrect");

            account.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            
            await _repo.SaveChangesAsync();
        }

        public async Task ForgotPasswordAsync(ForgotPasswordRequestDto request)
        {
            var account = await _repo.GetByEmailAsync(request.Email);
            if (account == null)
                return; // Do not reveal whether email exists; always return success to caller

            if (account.Status != "Active")
                return;

            var token = Guid.NewGuid().ToString("N");
            var expiry = DateTime.UtcNow.AddMinutes(_resetSettings.TokenExpiryMinutes);
            await _repo.SetPasswordRecoveryTokenAsync(account.AccountID, token, expiry);
            await _emailService.SendPasswordResetEmailAsync(account.Email, token);
        }

        public async Task ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            var account = await _repo.GetByPasswordRecoveryTokenAsync(request.Token);
            if (account == null)
                throw new BadRequestException("Invalid or expired reset link.");

            if (account.TokenExpiry == null || account.TokenExpiry.Value < DateTime.UtcNow)
                throw new BadRequestException("Reset link has expired. Please request a new one.");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _repo.UpdatePasswordAsync(account.AccountID, hashedPassword);

            // Send confirmation email
            await _emailService.SendPasswordResetConfirmationEmailAsync(account.Email);
        }
    }
}
