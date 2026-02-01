using ProductAPI.DTOs;
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
	}
}
