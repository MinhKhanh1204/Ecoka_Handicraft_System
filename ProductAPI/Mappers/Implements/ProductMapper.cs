using ProductAPI.DTOs;
using ProductAPI.Models;

namespace ProductAPI.Mappers.Implements
{
    public class ProductMapper : IProductMapper
    {
        public ProductDetailResponseDto ToDto(Product product, string mainImage)
        {
            return new ProductDetailResponseDto
            {
                ProductId = product.ProductID,
                ProductName = product.ProductName,
                Description = product.Description,
                Material = product.Material,
                Price = product.Price,
                Discount = product?.Discount ?? 0,
                FinalPrice = product.Price - ((product?.Discount ?? 0) / 100 * product.Price),
                StockQuantity = product.StockQuantity,
                Status = product.Status,

                Category = new CategoryDto
                {
                    CategoryID = product.Category.CategoryID,
                    CategoryName = product.Category.CategoryName
                },

                MainImage = mainImage,
                Images = product.ProductImages?
                            .Select(x => x.ImageURL)
                            .ToList()
            };
        }

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
