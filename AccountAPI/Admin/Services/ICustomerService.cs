using AccountAPI.Admin.DTOs;

namespace AccountAPI.Admin.Services
{
    public interface ICustomerService
    {
        Task<List<CustomerDto>> GetAllAsync();
        Task<CustomerDto?> GetByIdAsync(string customerId);
        Task<List<CustomerDto>> SearchAsync(string? keyword, string? status);
        Task<bool> UpdateStatusAsync(string customerId, string status);
    }
}
