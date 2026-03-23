using AccountAPI.DTOs;
using AccountAPI.Exceptions;
using AccountAPI.Helpers;
using AccountAPI.Models;
using AccountAPI.Repositories;
using AutoMapper;
using CloudinaryDotNet.Core;

namespace AccountAPI.Services.Implements
{
    public class AuthService : IAuthService
    {
        private readonly IAccountRepository _repo;
        private readonly JwtTokenHelper _jwt;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IEmailService _emailService;
        private readonly PasswordResetSettings _resetSettings;
        private readonly IMapper _mapper;

        public AuthService(
            IAccountRepository repo,
            JwtTokenHelper jwt,
            IMapper mapper,
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

        public async Task<LoginResponseDto> Login(LoginRequestDto request)
        {
            var account = await _repo.GetByEmailAsync(request.Email);
            if (account == null)
                throw new UnauthorizedException("Invalid email or password");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, account.Password))
                throw new UnauthorizedException("Invalid email or password");

            if (account.Status != "Active")
                throw new ForbiddenException("Account is inactive");

            var token = _jwt.GenerateToken(account);

            return _mapper.Map<LoginResponseDto>(token);
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
                throw new BadRequestException("Register fail");
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

        public async Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request)
        {
            var account = await _repo.GetByEmailAsync(request.Email);
            if (account == null)
            {
                // Email not found ? still return success (prevent enumeration),
                // but send an invitation email to register.
                var registerUrl = $"{_resetSettings.ClientBaseUrl.TrimEnd('/')}/Account/Register";
                try
                {
                    await _emailService.SendUnregisteredEmailAsync(request.Email, registerUrl);
                }
                catch
                {
                    // Swallow email-sending failures so the response is always the same.
                }
                return new ForgotPasswordResponseDto
                {
                    EmailExists = false,
                    Message = "Email nay chua duoc dang ki tai he thong. Lien he dang ki ben duoi.",
                    RegisterUrl = registerUrl
                };
            }

            if (account.Status != "Active")
            {
                return new ForgotPasswordResponseDto
                {
                    EmailExists = false,
                    Message = "Tai khoan cua ban da bi khoa. Vui long lien he ho tro."
                };
            }

            var token = Guid.NewGuid().ToString("N");
            var expiry = DateTime.UtcNow.AddMinutes(_resetSettings.TokenExpiryMinutes);
            await _repo.SetPasswordRecoveryTokenAsync(account.AccountID, token, expiry);
            await _emailService.SendPasswordResetEmailAsync(account.Email, token);

            return new ForgotPasswordResponseDto
            {
                EmailExists = true,
                Message = "Mot lien ket dat lai mat khau da duoc gui den email cua ban. Vui long kiem tra hop thu."
            };
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

		public async Task<ProfileResponseDto> GetProfileAsync(string accountId)
		{
			var account = await _repo.GetProfileAsync(accountId);

			if (account == null)
				throw new BadRequestException("Account not found");

			return new ProfileResponseDto
			{
				AccountId = account.AccountID,
				Username = account.Username,
				Email = account.Email,
				Avatar = account.Avatar,

				FullName = account.Customer?.FullName!,
				DateOfBirth = account.Customer?.DateOfBirth,
				Gender = account.Customer?.Gender,
				Phone = account.Customer?.Phone,
				Address = account.Customer?.Address
			};
		}

		public async Task UpdateProfileAsync(string accountId, UpdateProfileRequestDto request)
		{
			var account = await _repo.GetProfileAsync(accountId);

			if (account == null)
				throw new BadRequestException("Account not found");

			var customer = account.Customer;

			if (customer == null)
				throw new BadRequestException("Customer not found");

			// update customer
			customer.FullName = request.FullName;
			customer.DateOfBirth = request.DateOfBirth;
			customer.Gender = request.Gender;
			customer.Phone = request.Phone;
			customer.Address = request.Address;

			// upload avatar n?u có
			if (request.Avatar != null)
			{
				account.Avatar = await _cloudinaryService.UploadImageAsync(request.Avatar);
			}

			await _repo.UpdateAsync();
		}

        public async Task<LoginResponseDto> LoginSocial(SocialLoginRequestDto request)
        {
            var account = await _repo.GetByEmailAsync(request.Email);
            var token = "";
            if (account == null)
            {
                using var transaction = await _repo.BeginTransactionAsync();

                try
                {
                    var accountId = await _repo.GenerateAccountIdAsync();

                    var newAccount = new Account
                    {
                        AccountID = accountId,
                        Username = request.Email,
                        Email = request.Email,
                        Password = BCrypt.Net.BCrypt.HashPassword("Abc@123!"),
                        CreatedAt = DateTime.UtcNow,
                        Status = "Active"
                    };

                    await _repo.AddAccountAsync(newAccount);

                    var customer = new Customer
                    {
                        CustomerID = accountId,
                        FullName = request.Email,
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
                    token = _jwt.GenerateToken(newAccount);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw new BadRequestException("Register fail");
                }
            }
            else { 
                token = _jwt.GenerateToken(account); 
            }

            return _mapper.Map<LoginResponseDto>(token);
        }
    }
}
