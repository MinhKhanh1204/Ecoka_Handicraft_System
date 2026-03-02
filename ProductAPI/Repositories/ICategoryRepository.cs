using ProductAPI.DTOs;
using ProductAPI.Models;

namespace ProductAPI.Repositories
{
    public interface ICategoryRepository
    {
        List<Category> GetAll();
    }
}
