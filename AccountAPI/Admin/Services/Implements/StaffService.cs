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
                    Avatar = s.StaffNavigation.Avatar,
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
                Avatar = staff.StaffNavigation?.Avatar,
                Gender = staff.Gender,
                CitizenId = staff.CitizenId,
                DateOfBirth = staff.DateOfBirth,
                Status = staff.Status == "Active",
                HireDate = staff.HireDate
            };
        }

        public async Task<bool> CreateStaffAsync(CreateStaffDto dto)
        {
            // ===== VALIDATION =====

            if (await _repo.EmailExistsAsync(dto.Email))
                throw new InvalidOperationException("EMAIL_EXISTS");

            if (await _repo.PhoneExistsAsync(dto.Phone))
                throw new InvalidOperationException("PHONE_EXISTS");

            if (!string.IsNullOrEmpty(dto.CitizenId))
            {
                var existsCitizen = _repo.GetAll()
                    .Any(s => s.CitizenId == dto.CitizenId);

                if (existsCitizen)
                    throw new InvalidOperationException("CITIZENID_EXISTS"); // ✅ FIX
            }

            if (dto.DateOfBirth.HasValue)
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var age = today.Year - dto.DateOfBirth.Value.Year;

                if (dto.DateOfBirth.Value > today.AddYears(-age))
                    age--;

                if (age < 18)
                    throw new InvalidOperationException("INVALID_AGE");
            }

            // ===== USERNAME AUTO =====
            string baseUsername = dto.Email.Split('@')[0];
            string username = baseUsername;
            int suffix = 1;

            while (await _repo.UsernameExistsAsync(username))
            {
                username = baseUsername + suffix++;
            }

            await _repo.BeginTransactionAsync();

            try
            {
                var accountId = await _repo.GenerateStaffAccountIdAsync();

                var account = new Account
                {
                    AccountID = accountId,
                    Username = username,
                    Email = dto.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    CreatedAt = DateTime.UtcNow,
                    Status = "Active",
                    Avatar = dto.Avatar
                };

                await _repo.AddAccountAsync(account);

                var role = await _repo.GetRoleByIdAsync(dto.RoleID);
                if (role == null)
                    throw new InvalidOperationException("ROLE_NOT_FOUND");

                await _repo.AddUserRoleAsync(new UserRole
                {
                    AccountID = accountId,
                    RoleID = role.RoleID,
                    Status = "Active"
                });

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
            if (staff == null) return false;

            // ===== VALIDATION =====
            if (dto.DateOfBirth.HasValue)
            {
                var age = DateTime.Now.Year - dto.DateOfBirth.Value.Year;
                if (age < 18)
                    throw new InvalidOperationException("INVALID_AGE");
            }

            if (!string.IsNullOrEmpty(dto.CitizenId))
            {
                var existsCitizen = _repo.GetAll()
                    .Any(s => s.CitizenId == dto.CitizenId && s.StaffId != dto.StaffId);

                if (existsCitizen)
                    throw new InvalidOperationException("CITIZENID_EXISTS");
            }

            // ===== UPDATE =====
            staff.FullName = dto.FullName;
            staff.Phone = dto.Phone;
            staff.Address = dto.Address;
            staff.Gender = dto.Gender;
            staff.DateOfBirth = dto.DateOfBirth;
            staff.CitizenId = dto.CitizenId;
            staff.Status = dto.Status ? "Active" : "Deleted";

            // Sync Account
            if (staff.StaffNavigation != null)
            {
                staff.StaffNavigation.Status = dto.Status ? "Active" : "Deleted";

                if (!string.IsNullOrEmpty(dto.Avatar))
                {
                    staff.StaffNavigation.Avatar = dto.Avatar;
                }
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