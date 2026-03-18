using AccountAPI.Models;

namespace AccountAPI.Admin.Repositories
{
    public interface IStaffRepository
    {
        IQueryable<Staff> GetAll();

        Task<Staff?> GetByIdAsync(string id);

        Task<Role?> GetRoleByIdAsync(int roleId);

        Task<Role?> GetRoleByNameAsync(string roleName);

        Task AddAsync(Staff staff);

        void Update(Staff staff);
        Task SaveAsync();

        Task<string> GenerateStaffAccountIdAsync();
        Task AddAccountAsync(Account account);
        Task AddUserRoleAsync(UserRole userRole);

        Task<bool> EmailExistsAsync(string email);
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> PhoneExistsAsync(string phone); 
        Task<bool> CitizenIdExistsAsync(string citizenId); 

        Task AddRoleAsync(Role role);
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}