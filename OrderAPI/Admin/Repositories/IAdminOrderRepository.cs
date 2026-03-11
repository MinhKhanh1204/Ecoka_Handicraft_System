using OrderAPI.Admin.DTOs;

namespace OrderAPI.Admin.Repositories
{
    public interface IAdminOrderRepository
    {
        Task<IEnumerable<RevenueByMonthDto>> GetRevenueByYearAsync(int year);
    }
}
