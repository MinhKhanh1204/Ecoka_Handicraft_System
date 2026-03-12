using MVCApplication.Areas.Admin.DTOs;
using MVCApplication.Models.DTOs;

namespace MVCApplication.Areas.Admin.Services
{
    public interface IAdminOrderService
    {
        Task<IEnumerable<RevenueByMonthDto>> GetRevenueByYearAsync(int year);

        // STAFF
        Task<IEnumerable<Order>> GetAllOrdersForStaffAsync();
        Task<IEnumerable<Order>> SearchOrdersForStaffAsync(
            string? orderId,
            string? customerName,
            DateTime? from,
            DateTime? to,
            string? shippingStatus,
            string? paymentStatus);

        Task<bool> UpdateOrderStatusAsync(string orderId, string newStatus, string staffId);
        Task<Order?> GetOrderDetailForStaffAsync(string orderId);

        // GENERAL
        Task<Order?> GetByIdAsync(string orderId);
    }
}
