using ProductAPI.DTOs;

public interface ICategoryService
{
    Task<List<ReadCategoryDto>> GetAllAsync();
    Task<ReadCategoryDto?> GetByIdAsync(int id);
    Task<ReadCategoryDto> CreateAsync(CategoryCreateDto dto);
    Task<bool> UpdateAsync(int id, CategoryUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}

