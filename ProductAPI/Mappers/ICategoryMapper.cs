using ProductAPI.DTOs;
using ProductAPI.Models;

namespace ProductAPI.Mappers
{
    public interface ICategoryMapper
    {
        CategoryDto ToDto(Category product);
    }
}
