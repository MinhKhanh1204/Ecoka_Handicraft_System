using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrderAPI.Models;
using OrderAPI.Repositories;

namespace OrderAPI.Repositories.Implements
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly DBContext _context;

        public OrderItemRepository(DBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderItem>> GetAllAsync()
        {
            return await _context.OrderItems
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<OrderItem?> GetByIdAsync(int id)
        {
            if (id <= 0) return null;
            return await _context.OrderItems
                .AsNoTracking()
                .FirstOrDefaultAsync(oi => oi.OrderItemID == id);
        }

        public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId)) return Array.Empty<OrderItem>();
            return await _context.OrderItems
                .Where(oi => oi.OrderID == orderId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddAsync(OrderItem orderItem)
        {
            if (orderItem == null) throw new ArgumentNullException(nameof(orderItem));
            await _context.OrderItems.AddAsync(orderItem);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(OrderItem orderItem)
        {
            if (orderItem == null) throw new ArgumentNullException(nameof(orderItem));
            _context.OrderItems.Update(orderItem);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(OrderItem orderItem)
        {
            if (orderItem == null) throw new ArgumentNullException(nameof(orderItem));
            _context.OrderItems.Remove(orderItem);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteByIdAsync(int id)
        {
            if (id <= 0) return;
            var entity = await _context.OrderItems.FindAsync(id);
            if (entity == null) return;
            _context.OrderItems.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            if (id <= 0) return false;
            return await _context.OrderItems.AnyAsync(oi => oi.OrderItemID == id);
        }
    }
}
