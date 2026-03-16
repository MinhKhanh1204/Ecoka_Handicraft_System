using AccountAPI.Admin.DTOs;
using AccountAPI.CustomFormatter;
using static AccountAPI.Admin.DTOs.StaffDto;

namespace AccountAPI.Admin.Services
{
    public interface IStaffService
    {
        Task<PagedResult<ReadStaffDto>> GetStaffsAsync(StaffSearchDto search);

        Task<StaffDetailDto?> GetStaffDetailAsync(string id);

        Task<bool> CreateStaffAsync(CreateStaffDto dto);

        Task<bool> UpdateStaffAsync(UpdateStaffDto dto);

        Task<bool> DeleteStaffAsync(string id);
    }
}