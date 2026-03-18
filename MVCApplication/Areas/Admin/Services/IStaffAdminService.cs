using MVCApplication.Areas.Admin.DTOs;

namespace MVCApplication.Areas.Admin.Services
{
    public interface IStaffAdminService
    {
        Task<StaffPagedResult> GetStaffsAsync(string? keyword, string? role, bool? status, int page = 1, int pageSize = 10);

        Task<StaffDetailViewModel?> GetStaffDetailAsync(string id);

        Task<(bool Success, string? ErrorMessage)> CreateStaffAsync(CreateStaffViewModel model);

        Task<(bool Success, string? ErrorMessage)> UpdateStaffAsync(EditStaffViewModel model);

        Task<bool> DeleteStaffAsync(string id);
    }
}
