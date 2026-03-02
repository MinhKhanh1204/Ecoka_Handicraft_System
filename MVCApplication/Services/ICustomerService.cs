using MVCApplication.Models;

namespace MVCApplication.Services
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerViewModel>> GetAllAsync();
        Task<IEnumerable<CustomerViewModel>> SearchAsync(string? keyword, string? status);
        Task<CustomerViewModel?> GetByIdAsync(string id);
        Task<bool> UpdateStatusAsync(string id, string status);
    }
}
