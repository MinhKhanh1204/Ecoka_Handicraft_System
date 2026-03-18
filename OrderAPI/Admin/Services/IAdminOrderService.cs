using OrderAPI.Admin.DTOs;

namespace OrderAPI.Admin.Services
{
    public interface IAdminOrderService
    {
        Task<IEnumerable<RevenueByMonthDto>> GetRevenueByYearAsync(int year);
    }
}
