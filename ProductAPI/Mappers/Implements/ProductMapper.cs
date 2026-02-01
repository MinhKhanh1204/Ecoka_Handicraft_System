using ProductAPI.DTOs;
using ProductAPI.Models;

namespace ProductAPI.Mappers.Implements
{
	public class ProductMapper : IProductMapper
	{
		public ProductDto ToDto(Product p)
		{
			return new ProductDto
			{
				ProductID = p.ProductID,
				ProductName = p.ProductName,
				CategoryName = p.Category.CategoryName,
				OriginalPrice = p.Price,
				FinalPrice = p.Price - p.Discount,
				MainImage = p.ProductImages
					.FirstOrDefault(i => i.IsMain)?.ImageURL
			};
		}
	}
}
