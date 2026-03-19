using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrderAPI.Models;

namespace OrderAPI.Repositories
{
    public interface IOrderRepository
    {
        // ===== CUSTOMER =====
        Task<IEnumerable<Order>> GetOrdersByCustomerAsync(string customerId);

        Task<IEnumerable<Order>> SearchOrdersAsync(
            string customerId,
            string? orderId,
            DateTime? from,
            DateTime? to,
            string? paymentStatus,
            string? tabStatus);

        Task<Order?> GetOrderDetailAsync(string orderId);

        Task<bool> CancelOrderAsync(string orderId, string cancelReason);

        Task<bool> HasCustomerPurchasedProductAsync(string customerId, string productId);

        // ===== GENERAL =====
        Task<Order> CreateAsync(Order order);

        Task UpdatePaymentStatusAsync(
            string orderId,
            string paymentMethod,
            string paymentStatus,
            string note);

    }
}
