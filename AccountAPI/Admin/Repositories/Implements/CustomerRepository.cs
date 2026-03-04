using AccountAPI.Admin.DTOs;
using AccountAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountAPI.Admin.Repositories.Implements
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly DBContext _context;

        public CustomerRepository(DBContext context)
        {
            _context = context;
        }

        public async Task<List<Customer>> GetAllAsync()
        {
            return await _context.Customers
                .Include(c => c.Account)
                .Where(c => c.Account != null)
                .OrderByDescending(c => c.Account.CreatedAt)
                .ToListAsync();
        }

        public async Task<Customer?> GetByIdAsync(string customerId)
        {
            return await _context.Customers
                .Include(c => c.Account)
                .FirstOrDefaultAsync(c => c.CustomerID == customerId);
        }

        public async Task<List<Customer>> SearchAsync(string? keyword, string? status)
        {
            var query = _context.Customers.Include(c => c.Account).AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(c => 
                    (c.FullName != null && c.FullName.ToLower().Contains(keyword)) ||
                    (c.Phone != null && c.Phone.Contains(keyword)) ||
                    (c.Account != null && c.Account.Username.ToLower().Contains(keyword)) ||
                    (c.Account != null && c.Account.Email.ToLower().Contains(keyword))
                );
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(c => c.Status == status);
            }

            return await query.OrderByDescending(c => c.Account!.CreatedAt).ToListAsync();
        }

        public async Task<bool> UpdateStatusAsync(string customerId, string status)
        {
            var customer = await _context.Customers
                .Include(c => c.Account)
                .FirstOrDefaultAsync(c => c.CustomerID == customerId);
            
            if (customer == null || customer.Account == null) return false;

            customer.Status = status;
            customer.Account.Status = status;

            _context.Customers.Update(customer);
            _context.Accounts.Update(customer.Account);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
