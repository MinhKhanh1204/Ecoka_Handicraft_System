using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OrderAPI.Admin.DTOs;
using OrderAPI.Models;

namespace OrderAPI.Admin.Repositories.Implements
{
    public class AdminOrderRepository : IAdminOrderRepository
    {
        private readonly DBContext _context;

        public AdminOrderRepository(DBContext context )
        {
            _context = context;
        }

        public async Task<IEnumerable<RevenueByMonthDto>> GetRevenueByYearAsync(int year)
        {
            var paid = PaymentStatus.Paid.ToString();
            var completed = "Completed";

            var data = await _context.Orders.Include(o => o.OrderItems)
                .Where(o => o.OrderDate.HasValue
                            && o.OrderDate.Value.Year == year
                            && (o.PaymentStatus == paid || o.PaymentStatus == completed))
                .GroupBy(o => o.OrderDate!.Value.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Revenue = g.Sum(x => x.TotalAmount ?? 0)
                })
                .ToListAsync();

            var result = Enumerable.Range(1, 12)
                .Select(m => new RevenueByMonthDto
                {
                    Month = m,
                    Revenue = data.FirstOrDefault(x => x.Month == m)?.Revenue ?? 0
                });

            return result;
        }

        // ================= STAFF =================

        public async Task<IEnumerable<Order>> GetAllOrdersForStaffAsync()
        {
            return await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .Include(o => o.OrderItems)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> SearchOrdersForStaffAsync(
            string? orderId,
            string? customerId,
            DateTime? from,
            DateTime? to,
            string? shippingStatus,
            string? paymentStatus)
        {
            var query = _context.Orders.AsQueryable();

            if (!string.IsNullOrWhiteSpace(orderId))
                query = query.Where(o => o.OrderID.Contains(orderId));

            if (!string.IsNullOrWhiteSpace(customerId))
                query = query.Where(o => o.CustomerID.Contains(customerId));

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

            return await query
                .OrderByDescending(o => o.OrderDate)
                .Include(o => o.OrderItems)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> UpdateOrderStatusAsync(string orderId, string newStatus, string staffId)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderID == orderId);

            if (order == null)
                return false;

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

        public async Task<Order?> GetByIdAsync(string orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderID == orderId);
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

        // Updated method: returns bool and accepts optional staffId for audit
        public async Task<bool> UpdatePaymentStatusAsync(
            string orderId,
            string paymentStatus,
            string? note,
            string? staffId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            // If already the same status, optionally update note/staff and return success
            if (string.Equals(order.PaymentStatus, paymentStatus, StringComparison.OrdinalIgnoreCase))
            {
                var changed = false;
                if (!string.IsNullOrWhiteSpace(note) && !string.Equals(order.Note, note, StringComparison.Ordinal))
                {
                    order.Note = note;
                    changed = true;
                }
                if (!string.IsNullOrWhiteSpace(staffId) && !string.Equals(order.StaffID, staffId, StringComparison.Ordinal))
                {
                    order.StaffID = staffId;
                    changed = true;
                }
                if (changed)
                {
                    order.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
                return true;
            }

            if (Enum.TryParse<PaymentStatus>(paymentStatus, true, out var ps))
                order.PaymentStatus = ps.ToString();
            else
                order.PaymentStatus = paymentStatus;

            if (!string.IsNullOrWhiteSpace(note))
                order.Note = note;

            if (!string.IsNullOrWhiteSpace(staffId))
                order.StaffID = staffId;

            // allow refund even if shipping status is Cancelled
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
