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


        // ===== STAFF =====
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


        // ===== GENERAL =====
        Task<Order> CreateAsync(Order order);

        Task UpdatePaymentStatusAsync(
            string orderId,
            string paymentMethod,
            string paymentStatus,
            string note);

        Task<Order?> GetByIdAsync(string orderId);
    }
}
