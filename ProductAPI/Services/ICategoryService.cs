using ProductAPI.DTOs;
using ProductAPI.Models;
using System.ComponentModel;

namespace ProductAPI.Services
{
    public interface ICategoryService
    {
        List<CategoryDto> GetAllCategories();
    }
}
