using Microsoft.EntityFrameworkCore;
using ProductAPI.admin.DTOs;
using ProductAPI.admin.Repositories;
using ProductAPI.admin.Services;
using ProductAPI.Models;

namespace ProductAPI.admin.Services.Implements
{
    public class ProductAdminService : IProductAdminService
    {
        private readonly IProductAdminRepository _repository;

        public ProductAdminService(IProductAdminRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ProductListDto>> GetAllAsync()
        {
            return await _repository.GetQueryable()
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Select(p => new ProductListDto
                {
                    ProductID = p.ProductID,
                    ProductName = p.ProductName,
                    CategoryName = p.Category.CategoryName,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    Status = p.Status,
                    MainImage = p.ProductImages
                        .Where(i => i.IsMain)
                        .Select(i => i.ImageURL)
                        .FirstOrDefault()
                })
                .ToListAsync();
        }

        public async Task<List<ProductListDto>> SearchAsync(string keyword)
        {
            return await _repository.GetQueryable()
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p =>
                    p.ProductName.Contains(keyword) ||
                    p.Status.Contains(keyword) ||
                    p.Category.CategoryName.Contains(keyword)
                )
                .Select(p => new ProductListDto
                {
                    ProductID = p.ProductID,
                    ProductName = p.ProductName,
                    CategoryName = p.Category.CategoryName,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    Status = p.Status,
                    MainImage = p.ProductImages
                        .Where(i => i.IsMain)
                        .Select(i => i.ImageURL)
                        .FirstOrDefault()
                })
                .ToListAsync();
        }

        public async Task<ProductDetailDto> GetByIdAsync(string id)
        {
            var product = await _repository.GetByIdAsync(id);

            if (product == null)
                throw new Exception("Product not found");

            return new ProductDetailDto
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                Description = product.Description,
                Material = product.Material,
                Price = product.Price,
                Discount = product.Discount,
                StockQuantity = product.StockQuantity,
                Status = product.Status,
                CreatedAt = product.CreatedAt,
                CategoryName = product.Category.CategoryName,
                Images = product.ProductImages
                    .Select(i => i.ImageURL)
                    .ToList()
            };
        }

        public async Task CreateAsync(CreateProductDto dto)
        {
            if (dto.Images == null || dto.Images.Count != 4)
                throw new Exception("Product must contain exactly 4 images.");

            if (dto.Images.Count(i => i.IsMain) != 1)
                throw new Exception("Product must have exactly 1 main image.");

            var prefix = "ECOKA";

            var lastProduct = await _repository.GetQueryable()
                .Where(p => p.ProductID.StartsWith(prefix))
                .OrderByDescending(p => p.ProductID)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (lastProduct != null)
            {
                var numberPart = lastProduct.ProductID.Substring(prefix.Length);
                nextNumber = int.Parse(numberPart) + 1;
            }

            var newProductID = $"{prefix}{nextNumber:D3}";

            var product = new Product
            {
                ProductID = newProductID,
                CategoryID = dto.CategoryID,
                ProductName = dto.ProductName,
                Description = dto.Description,
                Material = dto.Material,
                Price = dto.Price,
                Discount = dto.Discount,
                StockQuantity = dto.StockQuantity,
                CreatedAt = DateTime.UtcNow,
                Status = "Pending"
            };

            await _repository.AddAsync(product);

            foreach (var img in dto.Images)
            {
                product.ProductImages.Add(new ProductImage
                {
                    ProductID = newProductID,
                    ImageURL = img.ImageUrl,
                    IsMain = img.IsMain
                });
            }

            await _repository.SaveChangesAsync();
        }

        public async Task UpdateAsync(string id, UpdateProductDto dto)
        {
            var product = await _repository.GetByIdAsync(id);

            if (product == null)
                throw new Exception("Product not found");

            product.CategoryID = dto.CategoryID;
            product.ProductName = dto.ProductName;
            product.Description = dto.Description;
            product.Material = dto.Material;
            product.Price = dto.Price;
            product.Discount = dto.Discount;
            product.StockQuantity = dto.StockQuantity;
            product.Status = dto.Status;

            if (dto.Images != null && dto.Images.Any())
            {
                if (dto.Images.Count != 4)
                    throw new Exception("Product must contain exactly 4 images.");

                if (dto.Images.Count(i => i.IsMain) != 1)
                    throw new Exception("Product must have exactly 1 main image.");

                product.ProductImages.Clear();

                foreach (var img in dto.Images)
                {
                    product.ProductImages.Add(new ProductImage
                    {
                        ProductID = id,
                        ImageURL = img.ImageUrl,
                        IsMain = img.IsMain
                    });
                }
            }

            _repository.Update(product);
            await _repository.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var product = await _repository.GetByIdAsync(id);

            if (product == null)
                throw new Exception("Product not found");

            product.Status = "Inactive";

            _repository.Update(product);
            await _repository.SaveChangesAsync();
        }
    }
}