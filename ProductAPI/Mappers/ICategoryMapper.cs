using ProductAPI.DTOs;
using ProductAPI.Models;

namespace ProductAPI.Admin.Mappers
{
    public interface ICategoryMapper
    {
        CategoryDto ToDto(Category product);
    }
}
