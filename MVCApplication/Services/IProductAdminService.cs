using MVCApplication.Models;

namespace MVCApplication.Services
{
    public interface IProductAdminService
    {
        Task<List<ProductListDto>> GetAllAsync();
        Task<List<ProductListDto>> SearchAsync(string keyword);
        Task<ProductDetailDto?> GetByIdAsync(string id);
        Task<bool> CreateAsync(ProductCreateDto dto);
        Task<bool> UpdateAsync(string id, ProductUpdateDto dto);
        Task<bool> ApproveAsync(string id);
        Task<bool> RejectAsync(string id);
        Task<bool> DeleteAsync(string id);
    }
}
