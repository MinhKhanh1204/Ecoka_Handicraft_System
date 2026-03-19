using MVCApplication.Areas.Admin.DTOs;
using MVCApplication.CustomFormatter;

namespace MVCApplication.Areas.Admin.Services
{
    public interface IProductAdminService
    {
        Task<PagedResult<ReadProductDto>> GetPagedAsync(string? keyword, string? status, int pageNumber, int pageSize);
        Task<ProductDetailDto?> GetByIdAsync(string id);
        Task<bool> CreateAsync(CreateProductDto dto);
        Task<bool> UpdateAsync(string id, UpdateProductDto dto);
        Task<bool> DeleteAsync(string id);
    }
}