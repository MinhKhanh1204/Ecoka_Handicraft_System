using ProductAPI.DTOs;
using ProductAPI.Mappers;
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

        public async Task<List<ReadCategoryDto>> GetAllAsync()
        {
            var categories = await _repo.GetAllAsync();
            return categories.Select(c => _mapper.ToDto(c)).ToList();
        }

        public async Task<ReadCategoryDto?> GetByIdAsync(int id)
        {
            var category = await _repo.GetByIdAsync(id);
            return category != null ? _mapper.ToDto(category) : null;
        }

        public async Task<ReadCategoryDto> CreateAsync(CategoryCreateDto dto)
        {
            var category = _mapper.ToEntity(dto);

            await _repo.AddAsync(category);
            await _repo.SaveChangesAsync();

            return _mapper.ToDto(category);
        }

        public async Task<bool> UpdateAsync(int id, CategoryUpdateDto dto)
        {
            var category = await _repo.GetByIdAsync(id);
            if (category == null) return false;

            _mapper.UpdateEntity(dto, category);
            _repo.Update(category);

            return await _repo.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _repo.GetByIdAsync(id);
            if (category == null) return false;

            _repo.Delete(category);
            return await _repo.SaveChangesAsync();
        }
    }

}
