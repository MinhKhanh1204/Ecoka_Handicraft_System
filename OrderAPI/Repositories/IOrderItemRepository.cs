using System.Collections.Generic;
using System.Threading.Tasks;
using OrderAPI.Models;

namespace OrderAPI.Repositories
{
    public interface IOrderItemRepository
    {
        Task<IEnumerable<OrderItem>> GetAllAsync();
        Task<OrderItem?> GetByIdAsync(int id);
        Task<IEnumerable<OrderItem>> GetByOrderIdAsync(string orderId);
        Task AddAsync(OrderItem orderItem);
        Task UpdateAsync(OrderItem orderItem);
        Task DeleteAsync(OrderItem orderItem);
        Task DeleteByIdAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
