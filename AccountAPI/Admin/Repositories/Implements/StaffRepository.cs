using AccountAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AccountAPI.Admin.Repositories.Implements
{
    public class StaffRepository : IStaffRepository
    {
        private readonly DBContext _context;
        private IDbContextTransaction? _transaction;

        public StaffRepository(DBContext context)
        {
            _context = context;
        }

        // 🔹 Lấy tất cả staff (include account + role)
        public IQueryable<Staff> GetAll()
        {
            return _context.Staffs
                .Include(s => s.StaffNavigation)
                    .ThenInclude(a => a.UserRoles)
                        .ThenInclude(ur => ur.Role)
                .AsQueryable();
        }

        // 🔹 Lấy staff theo ID
        public async Task<Staff?> GetByIdAsync(string id)
        {
            return await _context.Staffs
                .Include(s => s.StaffNavigation)
                    .ThenInclude(a => a.UserRoles)
                        .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(s => s.StaffId == id);
        }

        // 🔹 Lấy role theo ID
        public async Task<Role?> GetRoleByIdAsync(int roleId)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleID == roleId);
        }

        // 🔹 Lấy role theo tên
        public async Task<Role?> GetRoleByNameAsync(string roleName)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName.ToLower() == roleName.ToLower());
        }

        // 🔹 Thêm staff
        public async Task AddAsync(Staff staff)
        {
            await _context.Staffs.AddAsync(staff);
        }

        // 🔹 Cập nhật staff
        public void Update(Staff staff)
        {
            _context.Staffs.Update(staff);
        }

        // 🔹 Lưu thay đổi
        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        // 🔹 Account-related methods
        public async Task<string> GenerateStaffAccountIdAsync()
        {
            var lastAccount = await _context.Accounts
                .Where(x => x.AccountID.StartsWith("STF"))
                .OrderByDescending(x => x.AccountID)
                .FirstOrDefaultAsync();

            if (lastAccount == null)
                return "STF001";

            int number = int.Parse(lastAccount.AccountID.Substring(3));
            return $"STF{(number + 1).ToString("D3")}";
        }

        public async Task AddAccountAsync(Account account)
        {
            await _context.Accounts.AddAsync(account);
        }

        public async Task AddUserRoleAsync(UserRole userRole)
        {
            await _context.UserRoles.AddAsync(userRole);
        }

        // 🔹 Kiểm tra tồn tại
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Accounts.AnyAsync(x => x.Email == email);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Accounts.AnyAsync(x => x.Username == username);
        }

        public async Task<bool> PhoneExistsAsync(string phone)
        {
            return await _context.Staffs.AnyAsync(x => x.Phone == phone);
        }

        public async Task<bool> CitizenIdExistsAsync(string citizenId)
        {
            return await _context.Staffs.AnyAsync(x => x.CitizenId == citizenId);
        }

        // 🔹 Role management
        public async Task AddRoleAsync(Role role)
        {
            await _context.Roles.AddAsync(role);
        }

        // 🔹 Transaction
        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }
}