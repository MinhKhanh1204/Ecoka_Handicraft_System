using OrderAPI.Admin.DTOs;
using OrderAPI.DTOs;

namespace OrderAPI.Admin.Services
{
    public interface IAdminOrderService
    {
        Task<IEnumerable<RevenueByMonthDto>> GetRevenueByYearAsync(int year);

        // ================= STAFF =================

        Task<IEnumerable<OrderReadDto>> GetAllOrdersForStaffAsync();

        Task<IEnumerable<OrderReadDto>> SearchOrdersForStaffAsync(
            string? orderId,
            string? customerId,
            DateTime? from,
            DateTime? to,
            string? shippingStatus,
            string? paymentStatus);

        Task<bool> UpdateOrderStatusAsync(string orderId, string newStatus, string staffId);

        Task<OrderReadDto?> GetOrderDetailForStaffAsync(string orderId);

        // ================= GENERAL =================

        Task<OrderReadDto?> GetByIdAsync(string orderId);
    }
}
