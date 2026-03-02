using ProductAPI.Admin.DTOs;

namespace ProductAPI.Admin.Services
{
    public interface IProductAdminService
    {
        Task<List<ProductListDto>> GetAllAsync();
        Task<List<ProductListDto>> SearchAsync(string keyword);
        Task<ProductDetailDto> GetByIdAsync(string id);

        Task CreateAsync(CreateProductDto dto);
        Task UpdateAsync(string id, UpdateProductDto dto);

        // Admin actions
        Task ApproveAsync(string id);
        Task RejectAsync(string id);
        Task DeleteAsync(string id);
    }
}