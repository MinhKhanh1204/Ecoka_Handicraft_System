using OrderAPI.DTOs;

namespace OrderAPI.Services
{
    public interface IOrderService
    {
        // ================= CUSTOMER =================

        Task<IEnumerable<OrderReadDto>> GetOrdersByCustomerAsync(string customerId);

        Task<IEnumerable<OrderReadDto>> SearchOrdersAsync(
            string customerId,
            string? orderId,
            DateTime? from,
            DateTime? to,
            string? paymentStatus,
            string? tabStatus);

        Task<OrderReadDto?> GetOrderDetailAsync(string orderId);

        Task<bool> CancelOrderAsync(string orderId, string cancelReason);

        Task<bool> HasCustomerPurchasedProductAsync(string customerId, string productId);

        // ================= STAFF =================

        Task<IEnumerable<OrderReadDto>> GetAllOrdersForStaffAsync();

        Task<IEnumerable<OrderReadDto>> SearchOrdersForStaffAsync(
            string? orderId,
            string? customerName,
            DateTime? from,
            DateTime? to,
            string? shippingStatus,
            string? paymentStatus);

        Task<bool> UpdateOrderStatusAsync(string orderId, string newStatus, string staffId);

        Task<OrderReadDto?> GetOrderDetailForStaffAsync(string orderId);

        // ================= GENERAL =================

        Task<OrderReadDto> CreateAsync(OrderCreateDto dto);

        Task UpdatePaymentStatusAsync(
            string orderId,
            string paymentMethod,
            string paymentStatus,
            string? note);

        Task<OrderReadDto?> GetByIdAsync(string orderId);
    }
}