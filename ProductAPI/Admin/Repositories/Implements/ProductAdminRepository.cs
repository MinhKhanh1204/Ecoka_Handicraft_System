using Microsoft.EntityFrameworkCore;
using ProductAPI.Models;

namespace ProductAPI.admin.Repositories.Implements
{
    public class ProductAdminRepository : IProductAdminRepository
    {
        private readonly DBContext _context;

        public ProductAdminRepository(DBContext context)
        {
            _context = context;
        }

        public IQueryable<Product> GetQueryable()
        {
            return _context.Products;
        }

        public async Task<Product?> GetByIdAsync(string id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.ProductID == id);
        }

        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
        }

        public void Update(Product product)
        {
            _context.Products.Update(product);
        }

        public void Remove(Product product)
        {
            _context.Products.Remove(product);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
