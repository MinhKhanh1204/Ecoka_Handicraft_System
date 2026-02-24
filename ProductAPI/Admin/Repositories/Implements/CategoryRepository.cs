using Microsoft.EntityFrameworkCore;
using ProductAPI.Models;

namespace ProductAPI.Admin.Repositories.Implements
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly DBContext _context;

        public CategoryRepository(DBContext context)
        {
            _context = context;
        }

        public async Task<List<Category>> GetAllAsync()
        {
            return await _context.Categories
                .Where(c => c.Status != "Deleted")
                .ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryID == id && c.Status != "Deleted");
        }

        public async Task<List<Category>> SearchAsync(string keyword)
        {
            return await _context.Categories
                .Where(c => c.Status != "Deleted" && 
                    (c.CategoryName.Contains(keyword)))
                .ToListAsync();
        }

        public async Task AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
        }

        public void Update(Category category)
        {
            _context.Categories.Update(category);
        }

        public void Delete(Category category)
        {
            // Soft delete: update the status instead of removing
            category.Status = "Deleted";
            _context.Categories.Update(category);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

    }
}
