using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProductAPI.Admin.DTOs;
using ProductAPI.CustomFormatter;
using ProductAPI.DTOs;
using ProductAPI.Exceptions;
using ProductAPI.Repositories;

namespace ProductAPI.Services.Implements
{
	public class ProductService : IProductService
	{
		private readonly IProductRepository _repo;
        private readonly IMapper _mapper;

        public ProductService(
			IProductRepository repo,
            IMapper mapper)
		{
			_repo = repo;
			_mapper = mapper;
		}

		public List<ProductDto> GetAllProducts()
		{
			return _repo.GetAll().Select(p => _mapper.Map<ProductDto>(p)).ToList();
		}

        public async Task<ProductDetailResponseDto> GetProductDetailAsync(string productId)
        {
            var product = await _repo.GetProductDetailAsync(productId);

            if (product == null)
                throw new BadRequestException("Product is not exists");

            var mainImage = product.ProductImages?
                .FirstOrDefault(x => x.IsMain == true)?.ImageURL;

            return _mapper.Map<ProductDetailResponseDto>(product, opt => opt.Items["MainImage"] = mainImage);
        }

        public async Task<bool> UpdateStockAsync(string productId, int quantityChange)
        {
            return await _repo.UpdateStockAsync(productId, quantityChange);
		public async Task<PagedResult<ProductDto>> FilterProductsAsync(ProductFilterRequestDto request)
		{
			var query = await _repo.GetQueryableAsync();

			// search theo tên
			if (!string.IsNullOrEmpty(request.TxtSearch))
			{
				query = query.Where(p => p.ProductName.Contains(request.TxtSearch));
			}

			// filter category
			if (!string.IsNullOrEmpty(request.CategoryId + "") && request.CategoryId != 0)
			{
				query = query.Where(p =>p.CategoryID == request.CategoryId);
			}

			var totalItems = await query.CountAsync();

			var products = await query
				.Skip((request.Page - 1) * request.PageSize)
				.Take(request.PageSize)
				.Select(p => _mapper.Map<ProductDto>(p))
				.ToListAsync();

			return PagedResult<ProductDto>.Create(
				products, totalItems, request.Page, request.PageSize);
		}

        public async Task<List<ProductDto>> GetTopDiscountProductsAsync(int top = 10)
        {
            var query = await _repo.GetQueryableAsync();
            var products = await query
                .Where(x => x.Discount > 0)
                .OrderByDescending(x => x.Discount)
                .Take(top)
				.Select(p => _mapper.Map<ProductDto>(p))
                .ToListAsync();

            return products;
        }
    }
}
