using AccountAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AccountAPI.Repositories.Implements
{
    public class AccountRepository : IAccountRepository
    {
        private readonly DBContext _context;

        public AccountRepository(DBContext context)
        {
            _context = context;
        }
        public Account? GetByUsername(string username)
        {
            return _context.Accounts
                .Include(a => a.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefault(a => a.Username == username);
        }
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Accounts.AnyAsync(x => x.Email == email);
        }

        public async Task<string> GenerateAccountIdAsync()
        {
            var lastAccount = await _context.Accounts
                .Where(x => x.AccountID.StartsWith("CUS"))
                .OrderByDescending(x => x.AccountID)
                .FirstOrDefaultAsync();

            if (lastAccount == null)
                return "CUS001";

            int number = int.Parse(lastAccount.AccountID.Substring(3));
            return $"CUS{(number + 1).ToString("D3")}";
        }

        public async Task AddAccountAsync(Account account)
            => await _context.Accounts.AddAsync(account);

        public async Task AddCustomerAsync(Customer customer)
            => await _context.Customers.AddAsync(customer);

        public async Task<Role?> GetCustomerRoleAsync()
            => await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Customer");

        public async Task AddUserRoleAsync(UserRole userRole)
            => await _context.UserRoles.AddAsync(userRole);

        public async Task SaveChangesAsync()
            => await _context.SaveChangesAsync();

        public async Task<IDbContextTransaction> BeginTransactionAsync()
            => await _context.Database.BeginTransactionAsync();

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Accounts.AnyAsync(x => x.Username == username);
        }
    }
}
