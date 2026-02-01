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
	}
}
