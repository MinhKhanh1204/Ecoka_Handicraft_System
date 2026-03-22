using ProductAPI.Admin.DTOs;
using ProductAPI.CustomFormatter;

namespace ProductAPI.Admin.Services
{
    public interface IProductAdminService
    {
        Task<PagedResult<ProductListDto>> GetPagedAsync(string? keyword, string? status, string? userRole, int pageNumber, int pageSize);
        Task<ProductDetailDto> GetByIdAsync(string id);
        Task CreateAsync(CreateProductDto dto);
        Task UpdateAsync(string id, UpdateProductDto dto);
        Task DeleteAsync(string id);
    }
}