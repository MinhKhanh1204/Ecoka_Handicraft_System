using ProductAPI.Models;

namespace ProductAPI.Repositories
{
	public interface IProductRepository
	{
		List<Product> GetAll();
        Task<Product> GetProductDetailAsync(string productId);
        Task<bool> UpdateStockAsync(string productId, int quantityChange);
    }
}
