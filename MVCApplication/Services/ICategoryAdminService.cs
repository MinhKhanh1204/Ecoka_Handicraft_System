using MVCApplication.Models;

namespace MVCApplication.Services
{
    public interface ICategoryAdminService
    {
        Task<List<CategoryDto>> GetAllAsync();
        Task<List<CategoryDto>> SearchAsync(string keyword);
        Task<CategoryDto?> GetByIdAsync(int id);
        Task<CategoryDto?> CreateAsync(CategoryCreateDto dto);
        Task<bool> UpdateAsync(int id, CategoryUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
