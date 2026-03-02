using ProductAPI.Models;

namespace ProductAPI.Admin.Repositories
{
    public interface IProductAdminRepository
    {
        IQueryable<Product> GetQueryable();

        Task<Product?> GetByIdAsync(string id);

        Task AddAsync(Product product);

        void Update(Product product);

        void Remove(Product product);

        Task SaveChangesAsync();
    }
}
