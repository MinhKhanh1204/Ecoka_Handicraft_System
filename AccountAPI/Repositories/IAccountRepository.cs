using AccountAPI.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace AccountAPI.Repositories
{
    public interface IAccountRepository
    {
        Account? GetByUsername(string username);

        Task<bool> EmailExistsAsync(string email);
        Task<bool> UsernameExistsAsync(string username);
        Task<string> GenerateAccountIdAsync();
        Task AddAccountAsync(Account account);
        Task AddCustomerAsync(Customer customer);
        Task<Role?> GetCustomerRoleAsync();
        Task AddUserRoleAsync(UserRole userRole);
        Task SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
