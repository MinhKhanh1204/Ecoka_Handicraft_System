using ProductAPI.Admin.DTOs;
using ProductAPI.Models;
namespace ProductAPI.Admin.Mappers
{
    public interface ICategoryMapper
    {
        ReadCategoryDto ToDto(Category category);
        Category ToEntity(CategoryCreateDto dto);
        void UpdateEntity(CategoryUpdateDto dto, Category entity);
    }
}
