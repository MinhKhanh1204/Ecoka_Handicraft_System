using ProductAPI.DTOs;
using ProductAPI.Models;

namespace ProductAPI.Admin.Mappers.Implements
{
    public class CategoryMapper : ICategoryMapper
    {
        public CategoryDto ToDto(Category category)
        {
            return new CategoryDto
            {
                CategoryID = category.CategoryID,
                CategoryName = category.CategoryName
            };
        }
    }
}
