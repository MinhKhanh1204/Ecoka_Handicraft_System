using AccountAPI.Admin.DTOs;
using AccountAPI.Models;

namespace AccountAPI.Admin.Repositories
{
    public interface ICustomerRepository
    {
        Task<List<Customer>> GetAllAsync();
        Task<Customer?> GetByIdAsync(string customerId);
        Task<List<Customer>> SearchAsync(string? keyword, string? status);
        Task<bool> UpdateStatusAsync(string customerId, string status);
    }
}
