using AutoMapper;
using ProductAPI.DTOs;
using ProductAPI.Repositories;

namespace ProductAPI.Services.Implements
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repo;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public List<CategoryDto> GetAllCategories()
        {
            return _repo.GetAll().Select(p => _mapper.Map<CategoryDto>(p)).ToList();
        }
    }
}
