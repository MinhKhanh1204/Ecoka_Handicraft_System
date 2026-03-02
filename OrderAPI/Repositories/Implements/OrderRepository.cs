using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrderAPI.Models;
using OrderAPI.Repositories;

namespace OrderAPI.Repositories.Implements
{
    public class OrderRepository : IOrderRepository
    {
        private readonly DBContext _context;

        public OrderRepository(DBContext context)
        {
            _context = context;
        }

        // ================= CUSTOMER =================

        public async Task<IEnumerable<Order>> GetOrdersByCustomerAsync(string customerId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.CustomerID == customerId)
                .OrderByDescending(o => o.OrderDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> SearchOrdersAsync(
            string customerId,
            string? orderId,
            DateTime? from,
            DateTime? to,
            string? paymentStatus,
            string? tabStatus)
        {
            var query = _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.CustomerID == customerId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(orderId))
                query = query.Where(o => o.OrderID.Contains(orderId));

            if (from.HasValue)
                query = query.Where(o => o.OrderDate >= from.Value);

            if (to.HasValue)
                query = query.Where(o => o.OrderDate <= to.Value);

            if (!string.IsNullOrWhiteSpace(paymentStatus))
            {
                // try to parse provided paymentStatus into enum; if parse succeeds use enum string
                if (Enum.TryParse<PaymentStatus>(paymentStatus, true, out var ps))
                    query = query.Where(o => o.PaymentStatus == ps.ToString());
                else
                    query = query.Where(o => o.PaymentStatus == paymentStatus);
            }

            if (!string.IsNullOrWhiteSpace(tabStatus))
            {
                var t = tabStatus.Trim().ToLowerInvariant();
                if (t == "pending")
                {
                    var pending = PaymentStatus.Pending.ToString();
                    var shippingPending = ShippingStatus.Pending.ToString();
                    query = query.Where(o =>
                        o.ShippingStatus == shippingPending ||
                        o.PaymentStatus == pending);
                }
                else if (t == "history")
                {
                    // treat Approved as delivered/complete in current enum
                    var approved = ShippingStatus.Approved.ToString();
                    var cancelled = ShippingStatus.Cancelled.ToString();
                    query = query.Where(o =>
                        o.ShippingStatus == approved ||
                        o.ShippingStatus == cancelled);
                }
            }

            return await query
                .OrderByDescending(o => o.OrderDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Order?> GetOrderDetailAsync(string orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderID == orderId);
        }

        public async Task<bool> CancelOrderAsync(string orderId, string cancelReason)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderID == orderId);

            if (order == null)
                return false;

            // Use enum-backed semantics (compare via parsed enum)
            var currentShipping = order.ShippingStatus;
            if (currentShipping == ShippingStatus.Cancelled.ToString() ||
                currentShipping == ShippingStatus.Approved.ToString())
                return false;

            order.ShippingStatus = ShippingStatus.Cancelled.ToString();
            order.PaymentStatus = PaymentStatus.Refunded.ToString();
            order.Note = (order.Note ?? "") +
                $" [Cancelled: {cancelReason} - {DateTime.UtcNow:yyyy-MM-dd HH:mm}]";
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasCustomerPurchasedProductAsync(string customerId, string productId)
        {
            var paid = PaymentStatus.Paid.ToString();
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.CustomerID == customerId &&
                            o.PaymentStatus == paid)
                .AnyAsync(o => o.OrderItems
                    .Any(oi => oi.ProductID == productId));
        }

        // ================= STAFF =================

        public async Task<IEnumerable<Order>> GetAllOrdersForStaffAsync()
        {
            return await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> SearchOrdersForStaffAsync(
            string? orderId,
            string? customerName,
            DateTime? from,
            DateTime? to,
            string? shippingStatus,
            string? paymentStatus)
        {
            var query = _context.Orders.AsQueryable();

            if (!string.IsNullOrWhiteSpace(orderId))
                query = query.Where(o => o.OrderID.Contains(orderId));

            if (from.HasValue)
                query = query.Where(o => o.OrderDate >= from.Value);

            if (to.HasValue)
                query = query.Where(o => o.OrderDate <= to.Value);

            if (!string.IsNullOrWhiteSpace(shippingStatus))
            {
                if (Enum.TryParse<ShippingStatus>(shippingStatus, true, out var ss))
                    query = query.Where(o => o.ShippingStatus == ss.ToString());
                else
                    query = query.Where(o => o.ShippingStatus == shippingStatus);
            }

            if (!string.IsNullOrWhiteSpace(paymentStatus))
            {
                if (Enum.TryParse<PaymentStatus>(paymentStatus, true, out var ps))
                    query = query.Where(o => o.PaymentStatus == ps.ToString());
                else
                    query = query.Where(o => o.PaymentStatus == paymentStatus);
            }

            // optional: join on customerName if customers table exists (left out here)
            return await query
                .OrderByDescending(o => o.OrderDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> UpdateOrderStatusAsync(string orderId, string newStatus, string staffId)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderID == orderId);

            if (order == null)
                return false;

            // try to interpret newStatus as ShippingStatus enum; fall back to raw string
            if (Enum.TryParse<ShippingStatus>(newStatus, true, out var ss))
                order.ShippingStatus = ss.ToString();
            else
                order.ShippingStatus = newStatus;

            order.StaffID = staffId;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Order?> GetOrderDetailForStaffAsync(string orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderID == orderId);
        }

        // ================= GENERAL =================

        public async Task<Order> CreateAsync(Order order)
        {
            order.OrderID = GenerateOrderId();
            order.OrderDate = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            // set default statuses if not provided
            if (string.IsNullOrWhiteSpace(order.PaymentStatus))
                order.PaymentStatus = PaymentStatus.Pending.ToString();
            if (string.IsNullOrWhiteSpace(order.ShippingStatus))
                order.ShippingStatus = ShippingStatus.Pending.ToString();

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return order;
        }

        private string GenerateOrderId()
        {
            return $"ORD{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        }

        public async Task<Order?> GetByIdAsync(string orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderID == orderId);
        }

        public async Task UpdatePaymentStatusAsync(
            string orderId,
            string paymentMethod,
            string paymentStatus,
            string note)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return;

            order.PaymentMethod = paymentMethod;

            if (Enum.TryParse<PaymentStatus>(paymentStatus, true, out var ps))
                order.PaymentStatus = ps.ToString();
            else
                order.PaymentStatus = paymentStatus;

            order.Note = note;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // Approve order (admin action) — set shipping status to Approved and optionally set staff
        public async Task<bool> ApproveOrderAsync(string orderId, string? approverStaffId = null)
        {
            if (string.IsNullOrWhiteSpace(orderId)) return false;

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderID == orderId);
            if (order == null) return false;

            order.ShippingStatus = ShippingStatus.Approved.ToString();
            if (!string.IsNullOrWhiteSpace(approverStaffId))
                order.StaffID = approverStaffId;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
