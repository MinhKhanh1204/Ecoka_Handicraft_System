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
        
        Task UpdatePaymentStatusAsync(string orderId,
            string paymentMethod,
            string paymentStatus,
            string? note);

        Task<bool> CancelOrderAsync(string orderId, string cancelReason);
        Task<OrderReadDto?> GetByIdAsync(string orderId);
        Task<bool> HasCustomerPurchasedProductAsync(string customerId, string productId);

        Task<OrderReadDto> CreateAsync(OrderCreateDto dto);

    }
}