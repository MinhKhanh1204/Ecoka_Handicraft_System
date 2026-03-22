using ProductAPI.Models;

namespace ProductAPI.Repositories
{
	public interface IProductRepository
	{
		List<Product> GetAll();
		Task<IQueryable<Product>> GetQueryableAsync();
		Task<Product> GetProductDetailAsync(string productId);
    }
}
