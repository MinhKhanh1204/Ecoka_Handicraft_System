using ProductAPI.DTOs;
using ProductAPI.Models;

public interface ICategoryMapper
{
    ReadCategoryDto ToDto(Category category);
    Category ToEntity(CategoryCreateDto dto);
    void UpdateEntity(CategoryUpdateDto dto, Category entity);
}
