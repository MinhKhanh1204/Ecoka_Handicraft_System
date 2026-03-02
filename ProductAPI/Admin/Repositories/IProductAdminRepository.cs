using ProductAPI.Models;

namespace ProductAPI.admin.Repositories
{
    public interface IProductAdminRepository
    {
        IQueryable<Product> GetQueryable();

        Task<Product?> GetByIdAsync(string id);

        Task<List<Product>> GetByStatusAsync(string status);

        Task AddAsync(Product product);

        void Update(Product product);

        void Remove(Product product);

        Task SaveChangesAsync();
    }
}