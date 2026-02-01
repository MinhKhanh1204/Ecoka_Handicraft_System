using ProductAPI.Models;

namespace ProductAPI.Repositories
{
	public interface IProductRepository
	{
		List<Product> GetAll();
	}
}
