using ProductAPI.Admin.DTOs;
using ProductAPI.Admin.Mappers;
using ProductAPI.Admin.Repositories;

namespace ProductAPI.Admin.Services.Implements
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repo;
        private readonly IProductAdminRepository _productRepo;
        private readonly ICategoryMapper _mapper;

        public CategoryService(ICategoryRepository repo, IProductAdminRepository productRepo, ICategoryMapper mapper)
        {
            _repo = repo;
            _productRepo = productRepo;
            _mapper = mapper;
        }

        public async Task<List<ReadCategoryDto>> GetAllAsync()
        {
            var categories = await _repo.GetAllAsync();
            return categories.Select(c => _mapper.ToDto(c)).ToList();
        }

        public async Task<List<ReadCategoryDto>> SearchAsync(string keyword)
        {
            var categories = await _repo.SearchAsync(keyword);
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
            category.Status = "Pending"; // Always start as Pending

            await _repo.AddAsync(category);
            await _repo.SaveChangesAsync();

            return _mapper.ToDto(category);
        }

        public async Task<bool> UpdateAsync(int id, CategoryUpdateDto dto)
        {
            var category = await _repo.GetByIdAsync(id);
            if (category == null) return false;

            var oldStatus = category.Status;
            _mapper.UpdateEntity(dto, category);
            _repo.Update(category);

            var result = await _repo.SaveChangesAsync();
            if (result && !string.Equals(oldStatus, "Inactive", StringComparison.OrdinalIgnoreCase) && 
                string.Equals(category.Status, "Inactive", StringComparison.OrdinalIgnoreCase))
            {
                await _productRepo.InactivateByCategoryIdAsync(id);
            }

            return result;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _repo.GetByIdAsync(id);
            if (category == null) return false;

            _repo.Delete(category);
            var result = await _repo.SaveChangesAsync();
            if (result)
            {
                await _productRepo.InactivateByCategoryIdAsync(id);
            }
            return result;
        }

        public async Task<bool> ApproveAsync(int id)
        {
            var category = await _repo.GetByIdAsync(id);
            if (category == null) return false;

            category.Status = "Active";
            _repo.Update(category);
            return await _repo.SaveChangesAsync();
        }

        public async Task<bool> RejectAsync(int id)
        {
            var category = await _repo.GetByIdAsync(id);
            if (category == null) return false;

            category.Status = "Rejected";
            _repo.Update(category);
            return await _repo.SaveChangesAsync();
        }
    }

}
