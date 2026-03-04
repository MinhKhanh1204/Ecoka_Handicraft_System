using Microsoft.EntityFrameworkCore;
using ProductAPI.DTOs;
using ProductAPI.Models;

namespace ProductAPI.Repositories.Implements
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly DBContext _context;

        public CategoryRepository(DBContext context)
        {
            _context = context;
        }

        public List<Category> GetAll()
        {
            return _context.Categories
                .Where(p => p.Status == "Active")
                .ToList();
        }
    }
}
