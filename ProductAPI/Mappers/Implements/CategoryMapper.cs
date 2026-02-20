using ProductAPI.DTOs;
using ProductAPI.Models;

public class CategoryMapper : ICategoryMapper
{
    public ReadCategoryDto ToDto(Category category)
    {
        return new ReadCategoryDto
        {
            CategoryID = category.CategoryID,
            CategoryName = category.CategoryName,
            Description = category.Description,
            Status = category.Status
        };
    }

    public Category ToEntity(CategoryCreateDto dto)
    {
        return new Category
        {
            CategoryName = dto.CategoryName,
            Description = dto.Description,
            Status = dto.Status
        };
    }

    public void UpdateEntity(CategoryUpdateDto dto, Category entity)
    {
        entity.CategoryName = dto.CategoryName;
        entity.Description = dto.Description;
        entity.Status = dto.Status;
    }
}
