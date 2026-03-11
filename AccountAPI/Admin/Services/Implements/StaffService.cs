using AccountAPI.Admin.Repositories;
using AccountAPI.Models;
using Microsoft.EntityFrameworkCore;
using static AccountAPI.Admin.DTOs.StaffDto;

namespace AccountAPI.Admin.Services.Implements
{
    public class StaffService : IStaffService
    {
        private readonly IStaffRepository _repo;

        public StaffService(IStaffRepository repo)
        {
            _repo = repo;
        }

        public async Task<PagedResult<ReadStaffDto>> GetStaffsAsync(StaffSearchDto search)
        {
            var query = _repo.GetAll();

            if (!string.IsNullOrEmpty(search.Keyword))
            {
                query = query.Where(s =>
                    s.FullName!.Contains(search.Keyword) ||
                    s.Phone!.Contains(search.Keyword) ||
                    s.StaffNavigation.Email.Contains(search.Keyword));
            }

            if (!string.IsNullOrEmpty(search.Role))
            {
                query = query.Where(s =>
                    s.StaffNavigation.UserRoles.Any(ur => ur.Role.RoleName == search.Role));
            }

            if (search.Status.HasValue)
            {
                string status = search.Status.Value ? "Active" : "Deleted";
                query = query.Where(s => s.Status == status);
            }

            int totalItems = await query.CountAsync();

            var staffs = await query
                .Skip((search.Page - 1) * search.PageSize)
                .Take(search.PageSize)
                .Select(s => new ReadStaffDto
                {
                    StaffId = s.StaffId,
                    FullName = s.FullName ?? "",
                    Email = s.StaffNavigation.Email,
                    Phone = s.Phone ?? "",
                    Role = s.StaffNavigation.UserRoles
                        .Select(ur => ur.Role.RoleName)
                        .FirstOrDefault() ?? "Staff",
                    Status = s.Status == "Active"
                })
                .ToListAsync();

            return new PagedResult<ReadStaffDto>
            {
                Items = staffs,
                TotalItems = totalItems,
                Page = search.Page,
                PageSize = search.PageSize
            };
        }

        public async Task<StaffDetailDto?> GetStaffDetailAsync(string id)
        {
            var staff = await _repo.GetByIdAsync(id);

            if (staff == null) return null;

            return new StaffDetailDto
            {
                StaffId = staff.StaffId,
                FullName = staff.FullName ?? "",
                Email = staff.StaffNavigation?.Email ?? "",
                Phone = staff.Phone ?? "",
                Address = staff.Address ?? "",
                Role = staff.StaffNavigation?.UserRoles
                    .Select(ur => ur.Role.RoleName)
                    .FirstOrDefault() ?? "Staff",
                Status = staff.Status == "Active",
                HireDate = staff.HireDate
            };
        }

        public async Task<bool> CreateStaffAsync(CreateStaffDto dto)
        {
            // Check email uniqueness
            if (await _repo.EmailExistsAsync(dto.Email))
                return false;

            // Use email prefix as username, ensure uniqueness
            string baseUsername = dto.Email.Split('@')[0];
            string username = baseUsername;
            int suffix = 1;
            while (await _repo.UsernameExistsAsync(username))
            {
                username = baseUsername + suffix;
                suffix++;
            }

            await _repo.BeginTransactionAsync();

            try
            {
                // 1. Generate Account ID
                var accountId = await _repo.GenerateStaffAccountIdAsync();

                // 2. Create Account
                var account = new Account
                {
                    AccountID = accountId,
                    Username = username,
                    Email = dto.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    CreatedAt = DateTime.UtcNow,
                    Status = "Active"
                };

                await _repo.AddAccountAsync(account);

                // 3. Assign Role
                var role = await _repo.GetRoleByNameAsync(dto.Role ?? "Staff");
                if (role == null)
                {
                    await _repo.RollbackTransactionAsync();
                    return false;
                }

                await _repo.AddUserRoleAsync(new UserRole
                {
                    AccountID = accountId,
                    RoleID = role.RoleID,
                    Status = "Active"
                });

                // 4. Create Staff profile
                var staff = new Staff
                {
                    StaffId = accountId,
                    FullName = dto.FullName,
                    Phone = dto.Phone,
                    Address = dto.Address,
                    Gender = dto.Gender,
                    CitizenId = dto.CitizenId,
                    DateOfBirth = dto.DateOfBirth,
                    HireDate = DateOnly.FromDateTime(DateTime.Now),
                    Status = "Active"
                };

                await _repo.AddAsync(staff);
                await _repo.SaveAsync();
                await _repo.CommitTransactionAsync();

                return true;
            }
            catch
            {
                await _repo.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> UpdateStaffAsync(UpdateStaffDto dto)
        {
            var staff = await _repo.GetByIdAsync(dto.StaffId);

            if (staff == null)
                return false;

            staff.FullName = dto.FullName;
            staff.Phone = dto.Phone;
            staff.Address = dto.Address;
            staff.Gender = dto.Gender;
            staff.CitizenId = dto.CitizenId;
            staff.DateOfBirth = dto.DateOfBirth;
            staff.Status = dto.Status ? "Active" : "Deleted";

            // Also update Account status to sync
            if (staff.StaffNavigation != null)
            {
                staff.StaffNavigation.Status = dto.Status ? "Active" : "Deleted";
            }

            _repo.Update(staff);
            await _repo.SaveAsync();

            return true;
        }

        public async Task<bool> DeleteStaffAsync(string id)
        {
            var staff = await _repo.GetByIdAsync(id);

            if (staff == null)
                return false;

            // Soft delete: mark as Deleted
            staff.Status = "Deleted";

            // Also disable the account
            if (staff.StaffNavigation != null)
            {
                staff.StaffNavigation.Status = "Deleted";
            }

            _repo.Update(staff);
            await _repo.SaveAsync();

            return true;
        }
    }
}