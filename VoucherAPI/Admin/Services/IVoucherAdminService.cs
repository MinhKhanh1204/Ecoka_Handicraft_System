using VoucherAPI.Admin.DTOs;
using VoucherAPI.CustomFormatter;

namespace VoucherAPI.Admin.Services
{
    public interface IVoucherAdminService
    {
        Task<PagedResult<VoucherListDto>> GetPagedAsync(string? keyword, string? status, string? sortBy, int pageNumber, int pageSize);
        Task<VoucherDetailDto?> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateVoucherDto dto);
        Task<bool> UpdateAsync(int id, UpdateVoucherDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
