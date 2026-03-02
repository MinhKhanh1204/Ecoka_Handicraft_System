using MVCApplication.Models;

namespace MVCApplication.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetOrdersByCustomerAsync(string customerId);
        Task<IEnumerable<Order>> SearchOrdersAsync(string customerId, string? orderId, DateTime? from, DateTime? to, string? paymentStatus, string? tabStatus);
        Task<Order?> GetOrderDetailAsync(string orderId);
        Task<bool> CancelOrderAsync(string orderId, string cancelReason);
        Task<bool> HasCustomerPurchasedProductAsync(string customerId, string productId);
        Task<Order> CreateAsync(OrderCreateDto dto);
        Task UpdatePaymentStatusAsync(string orderId, string paymentMethod, string paymentStatus, string? note);
        Task<Order?> GetByIdAsync(string orderId);
    }
}
