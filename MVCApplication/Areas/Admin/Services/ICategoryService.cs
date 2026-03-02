using MVCApplication.Areas.Admin.DTOs;

namespace MVCApplication.Areas.Admin.Services
{
    public interface ICategoryService
    {
        Task<IReadOnlyList<ReadCategoryDto>> GetAllAsync();
        Task<IReadOnlyList<ReadCategoryDto>> SearchAsync(string keyword);
        Task<ReadCategoryDto?> GetByIdAsync(int id);
        Task<ReadCategoryDto?> CreateAsync(CategoryCreateDto dto);
        Task<bool> UpdateAsync(int id, CategoryUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}

