using MVCApplication.Areas.Admin.DTOs;

namespace MVCApplication.Areas.Admin.Services
{
    public interface IAdminOrderService
    {
        Task<IEnumerable<RevenueByMonthDto>> GetRevenueByYearAsync(int year);
    }
}
