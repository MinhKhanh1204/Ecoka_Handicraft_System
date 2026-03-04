using ProductAPI.DTOs;
using ProductAPI.Exceptions;
using ProductAPI.Mappers;
using ProductAPI.Repositories;

namespace ProductAPI.Services.Implements
{
	public class ProductService : IProductService
	{
		private readonly IProductRepository _repo;
		private readonly IProductMapper _mapper;

		public ProductService(
			IProductRepository repo,
			IProductMapper mapper)
		{
			_repo = repo;
			_mapper = mapper;
		}

		public List<ProductDto> GetAllProducts()
		{
			return _repo.GetAll().Select(p => _mapper.ToDto(p)).ToList();
		}

        public async Task<ProductDetailResponseDto> GetProductDetailAsync(string productId)
        {
            var product = await _repo.GetProductDetailAsync(productId);

            if (product == null)
                throw new BadRequestException("Product is not exists");

            var mainImage = product.ProductImages?
                .FirstOrDefault(x => x.IsMain == true)?.ImageURL;

            return _mapper.ToDto(product, mainImage);
        }
    }
}
