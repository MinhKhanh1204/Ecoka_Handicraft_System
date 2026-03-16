using Microsoft.EntityFrameworkCore;
using OrderAPI.Admin.DTOs;
using OrderAPI.Models;

namespace OrderAPI.Admin.Repositories.Implements
{
    public class AdminOrderRepository : IAdminOrderRepository
    {
        private readonly DBContext _context;

        public AdminOrderRepository(DBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RevenueByMonthDto>> GetRevenueByYearAsync(int year)
        {
            var paid = PaymentStatus.Paid.ToString();
            var completed = "Completed";

            var data = await _context.Orders
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
    }
}
