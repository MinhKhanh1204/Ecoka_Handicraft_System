using Microsoft.EntityFrameworkCore;
using ProductAPI.Admin.DTOs;
using ProductAPI.Admin.Repositories;
using ProductAPI.Admin.Services;
using ProductAPI.Models;
using ProductAPI.CustomFormatter;
using ProductAPI.Services;

namespace ProductAPI.Admin.Services.Implements
{
    public class ProductAdminService : IProductAdminService
    {
        private readonly IProductAdminRepository _repository;
        private readonly ICloudinaryService _cloudinaryService;

        public ProductAdminService(IProductAdminRepository repository, ICloudinaryService cloudinaryService)
        {
            _repository = repository;
            _cloudinaryService = cloudinaryService;
        }

        // ================= GET PAGED =================
        public async Task<PagedResult<ProductListDto>> GetPagedAsync(string? keyword, string? status, string? userRole, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _repository.GetQueryable();

            // ================= FILTER =================

            if (!string.IsNullOrWhiteSpace(userRole) &&
                userRole.Equals("Staff", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.Status != ProductStatus.Rejected);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(p =>
                    EF.Functions.Like(p.ProductName, $"%{keyword}%") ||
                    (p.Category != null &&
                     EF.Functions.Like(p.Category.CategoryName, $"%{keyword}%"))
                );
            }

            if (!string.IsNullOrWhiteSpace(status) &&
                !status.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                var statusLower = status.Trim().ToLower();
                query = query.Where(p => p.Status.ToLower() == statusLower);
            }

            // ================= COUNT (chỉ filter) =================
            var totalCount = await query.CountAsync();

            // ================= ORDER ỔN ĐỊNH =================
            // Dùng ProductID làm key chính để đảm bảo thứ tự tuyệt đối
            query = query
                .OrderBy(p => p.ProductID);

            // ================= PAGING =================
            var items = await query
                .AsNoTracking()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductListDto
                {
                    ProductID = p.ProductID,
                    ProductName = p.ProductName,
                    CategoryName = p.Category != null
                        ? p.Category.CategoryName
                        : "Unknown",
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    Status = p.Status,
                    MainImage = p.ProductImages
                        .Where(i => i.IsMain)
                        .Select(i => i.ImageURL)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return PagedResult<ProductListDto>.Create(
                items, totalCount, pageNumber, pageSize);
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
                    .OrderByDescending(i => i.IsMain)
                    .Select(i => i.ImageURL)
                    .ToList()
            };
        }

        // ================= CREATE =================
        public async Task CreateAsync(CreateProductDto dto)
        {
            ValidateProduct(dto.ProductName, dto.Description, dto.Material,
                            dto.Price, dto.Discount, dto.StockQuantity);

            await ValidateImagesAsync(dto.Images, i => i.ImageUrl, i => i.ImageFile, i => i.IsMain);

            var prefix = "ECOKA";
            // ... (rest of ID generation logic)

            var lastProduct = await _repository.GetQueryable()
                .Where(p => p.ProductID.StartsWith(prefix))
                .OrderByDescending(p => p.ProductID)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (lastProduct != null)
            {
                var numberPart = lastProduct.ProductID.Substring(prefix.Length);
                if (int.TryParse(numberPart, out int currentNumber))
                {
                    nextNumber = currentNumber + 1;
                }
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
                var imageUrl = img.ImageUrl;
                if (img.ImageFile != null)
                {
                    imageUrl = await _cloudinaryService.UploadImageAsync(img.ImageFile);
                }

                product.ProductImages.Add(new ProductImage
                {
                    ProductID = newProductID,
                    ImageURL = imageUrl!,
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
                await ValidateImagesAsync(dto.Images, i => i.ImageUrl, i => i.ImageFile, i => i.IsMain);

                product.ProductImages.Clear();

                foreach (var img in dto.Images)
                {
                    var imageUrl = img.ImageUrl;
                    if (img.ImageFile != null)
                    {
                        imageUrl = await _cloudinaryService.UploadImageAsync(img.ImageFile);
                    }

                    product.ProductImages.Add(new ProductImage
                    {
                        ProductID = id,
                        ImageURL = imageUrl!,
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

        private async Task ValidateImagesAsync<T>(
            ICollection<T> images,
            Func<T, string?> imageUrlSelector,
            Func<T, IFormFile?> imageFileSelector,
            Func<T, bool> isMainSelector)
        {
            if (images == null || images.Count != 4)
                throw new Exception("Product must contain exactly 4 images.");

            if (images.Count(i => isMainSelector(i)) != 1)
                throw new Exception("Product must have exactly 1 main image.");

            foreach (var img in images)
            {
                if (string.IsNullOrWhiteSpace(imageUrlSelector(img)) && imageFileSelector(img) == null)
                    throw new Exception("Each image must have either a URL or a file.");
            }
        }
    }
}