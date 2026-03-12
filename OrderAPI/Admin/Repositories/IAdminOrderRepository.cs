using OrderAPI.Admin.DTOs;
using OrderAPI.Models;

namespace OrderAPI.Admin.Repositories
{
    public interface IAdminOrderRepository
    {
        Task<IEnumerable<RevenueByMonthDto>> GetRevenueByYearAsync(int year);

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

        Task<Order?> GetByIdAsync(string orderId);
    }
}
