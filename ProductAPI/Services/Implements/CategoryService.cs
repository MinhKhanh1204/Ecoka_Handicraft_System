using ProductAPI.DTOs;
using ProductAPI.Mappers;
using ProductAPI.Models;
using ProductAPI.Repositories;

namespace ProductAPI.Services.Implements
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repo;
        private readonly ICategoryMapper _mapper;

        public CategoryService(ICategoryRepository repo, ICategoryMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public List<CategoryDto> GetAllCategories()
        {
            return _repo.GetAll().Select(p => _mapper.ToDto(p)).ToList();
        }
    }
}
