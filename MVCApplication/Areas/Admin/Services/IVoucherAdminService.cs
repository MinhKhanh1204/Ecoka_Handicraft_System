using MVCApplication.Areas.Admin.DTOs;
using MVCApplication.CustomFormatter;

namespace MVCApplication.Areas.Admin.Services
{
    public interface IVoucherAdminService
    {
        Task<PagedResult<VoucherListDto>> GetPagedAsync(string? keyword, string? status, string? sortBy, int pageNumber, int pageSize);
        Task<VoucherDetailDto?> GetByIdAsync(int id);
        Task<bool> CreateAsync(CreateVoucherDto dto);
        Task<bool> UpdateAsync(int id, UpdateVoucherDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
