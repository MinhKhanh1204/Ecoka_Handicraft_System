using Microsoft.EntityFrameworkCore;
using ProductAPI.Admin.DTOs;
using ProductAPI.Admin.Repositories;
using ProductAPI.Admin.Services;
using ProductAPI.Models;

namespace ProductAPI.Admin.Services.Implements
{
    public class ProductAdminService : IProductAdminService
    {
        private readonly IProductAdminRepository _repository;

        public ProductAdminService(IProductAdminRepository repository)
        {
            _repository = repository;
        }

        // ================= GET ALL =================
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

        // ================= SEARCH =================
        public async Task<List<ProductListDto>> SearchAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return await GetAllAsync();

            keyword = keyword.Trim();

            return await _repository.GetQueryable()
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p =>
                    EF.Functions.Collate(p.ProductName, "Latin1_General_CI_AI").Contains(keyword) ||
                    EF.Functions.Collate(p.Status, "Latin1_General_CI_AI").Contains(keyword) ||
                    EF.Functions.Collate(p.Category.CategoryName, "Latin1_General_CI_AI").Contains(keyword)
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

        // ================= GET BY ID =================
        public async Task<ProductDetailDto> GetByIdAsync(string id)
        {
            var product = await _repository.GetQueryable()
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.ProductID == id);

            if (product == null)
                throw new Exception("Product not found.");

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

        // ================= CREATE =================
        public async Task CreateAsync(CreateProductDto dto)
        {
            ValidateProduct(dto.ProductName, dto.Description, dto.Material,
                            dto.Price, dto.Discount, dto.StockQuantity);

            ValidateImages(dto.Images, i => i.ImageUrl, i => i.IsMain);

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
                ProductName = dto.ProductName.Trim(),
                Description = dto.Description?.Trim(),
                Material = dto.Material?.Trim(),
                Price = dto.Price,
                Discount = dto.Discount,
                StockQuantity = dto.StockQuantity,
                CreatedAt = DateTime.UtcNow,
                Status = ProductStatus.Pending
            };

            foreach (var img in dto.Images)
            {
                product.ProductImages.Add(new ProductImage
                {
                    ProductID = newProductID,
                    ImageURL = img.ImageUrl,
                    IsMain = img.IsMain
                });
            }

            await _repository.AddAsync(product);
            await _repository.SaveChangesAsync();
        }

        // ================= UPDATE =================
        public async Task UpdateAsync(string id, UpdateProductDto dto)
        {
            var product = await _repository.GetQueryable()
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.ProductID == id);

            if (product == null)
                throw new Exception("Product not found.");

            ValidateProduct(dto.ProductName, dto.Description, dto.Material,
                            dto.Price, dto.Discount, dto.StockQuantity);

            product.CategoryID = dto.CategoryID;
            product.ProductName = dto.ProductName.Trim();
            product.Description = dto.Description?.Trim();
            product.Material = dto.Material?.Trim();
            product.Price = dto.Price;
            product.Discount = dto.Discount;
            product.StockQuantity = dto.StockQuantity;

            if (dto.Images != null && dto.Images.Any())
            {
                ValidateImages(dto.Images, i => i.ImageUrl, i => i.IsMain);

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

        // ================= ADMIN APPROVE / REJECT =================
        public async Task ApproveAsync(string id)
        {
            await ChangeStatusAsync(id, ProductStatus.Approved);
        }

        public async Task RejectAsync(string id)
        {
            await ChangeStatusAsync(id, ProductStatus.Rejected);
        }

        public async Task DeleteAsync(string id)
        {
            await ChangeStatusAsync(id, ProductStatus.Inactive);
        }

        private async Task ChangeStatusAsync(string id, string newStatus)
        {
            var product = await _repository.GetByIdAsync(id);

            if (product == null)
                throw new Exception("Product not found.");

            var allowedTransitions = new Dictionary<string, List<string>>
            {
                { ProductStatus.Pending, new List<string> { ProductStatus.Approved, ProductStatus.Rejected } },
                { ProductStatus.Rejected, new List<string> { ProductStatus.Approved } },
                { ProductStatus.Approved, new List<string> { ProductStatus.Inactive } },
                { ProductStatus.Inactive, new List<string>() }
            };

            if (!allowedTransitions.ContainsKey(product.Status) ||
                !allowedTransitions[product.Status].Contains(newStatus))
            {
                throw new Exception(
                    $"Cannot change status from {product.Status} to {newStatus}.");
            }

            product.Status = newStatus;

            _repository.Update(product);
            await _repository.SaveChangesAsync();
        }

        // ================= PRIVATE VALIDATION =================
        private void ValidateProduct(string name, string? description, string? material,
                                     decimal price, decimal discount, int stock)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length > 200)
                throw new Exception("Product name is required and must be less than 200 characters.");

            if (price <= 0)
                throw new Exception("Price must be greater than 0.");

            if (discount < 0 || discount > 100)
                throw new Exception("Discount must be between 0 and 100.");

            if (stock < 0)
                throw new Exception("Stock quantity cannot be negative.");
        }

        private void ValidateImages<T>(
            ICollection<T> images,
            Func<T, string> imageUrlSelector,
            Func<T, bool> isMainSelector)
        {
            if (images == null || images.Count != 4)
                throw new Exception("Product must contain exactly 4 images.");

            if (images.Count(i => isMainSelector(i)) != 1)
                throw new Exception("Product must have exactly 1 main image.");

            foreach (var img in images)
            {
                if (string.IsNullOrWhiteSpace(imageUrlSelector(img)))
                    throw new Exception("Image URL cannot be empty.");
            }
        }
    }

    public static class ProductStatus
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
        public const string Inactive = "Inactive";
    }
}