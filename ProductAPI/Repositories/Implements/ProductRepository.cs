using Microsoft.EntityFrameworkCore;
using ProductAPI.Models;

namespace ProductAPI.Repositories.Implements
{
	public class ProductRepository : IProductRepository
	{
		private readonly DBContext _context;

		public ProductRepository(DBContext context)
		{
			_context = context;
		}

		public List<Product> GetAll()
		{
			return _context.Products
				.Include(p => p.Category)
				.Include(p => p.ProductImages)
				.Where(p => p.Status == "Active")
				.ToList();
		}

        public async Task<Product> GetProductDetailAsync(string productId)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => p.Status == "Active")
                .FirstOrDefaultAsync(p => p.ProductID == productId);
        }

        public async Task<bool> UpdateStockAsync(string productId, int quantityChange)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            product.StockQuantity += quantityChange;

            // Optional: prevent negative stock if desired, but here we just update
            if (product.StockQuantity < 0) product.StockQuantity = 0;

            return await _context.SaveChangesAsync() > 0;
        }
    }
}
