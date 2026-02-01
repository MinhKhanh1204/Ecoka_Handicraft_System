using ProductAPI.DTOs;
using ProductAPI.Models;

namespace ProductAPI.Mappers
{
	public interface IProductMapper
	{
		ProductDto ToDto(Product product);
	}
}
