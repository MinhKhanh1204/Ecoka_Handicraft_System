using AccountAPI.Models;

namespace AccountAPI.Admin.Repositories
{
    public interface IStaffRepository
    {
        IQueryable<Staff> GetAll();

        Task<Staff?> GetByIdAsync(string id);

        Task AddAsync(Staff staff);

        void Update(Staff staff);

        Task SaveAsync();

        // Account-related methods for staff creation
        Task<string> GenerateStaffAccountIdAsync();

        Task<bool> EmailExistsAsync(string email);

        Task<bool> UsernameExistsAsync(string username);

        Task AddAccountAsync(Account account);

        Task AddUserRoleAsync(UserRole userRole);

        Task<Role?> GetRoleByNameAsync(string roleName);

        Task BeginTransactionAsync();

        Task CommitTransactionAsync();

        Task RollbackTransactionAsync();
    }
}