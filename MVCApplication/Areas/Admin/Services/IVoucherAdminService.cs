using MVCApplication.Areas.Admin.DTOs;
using MVCApplication.CustomFormatter;

namespace MVCApplication.Areas.Admin.Services
{
    public interface IVoucherAdminService
    {
        Task<PagedResult<VoucherListDto>> GetPagedAsync(string? keyword, string? status, string? sortBy, int pageNumber, int pageSize);
        Task<VoucherDetailDto?> GetByIdAsync(int id);
        Task<VoucherOperationResult> CreateAsync(CreateVoucherDto dto, bool createdByAdmin);
        Task<VoucherOperationResult> ApproveAsync(int id);
        Task<VoucherOperationResult> UpdateAsync(int id, UpdateVoucherDto dto);
        Task<VoucherOperationResult> DeleteAsync(int id);
    }
}
