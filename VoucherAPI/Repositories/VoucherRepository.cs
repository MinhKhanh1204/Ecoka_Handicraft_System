using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using VoucherAPI.Models;

namespace VoucherAPI.Repositories
{
    public class VoucherRepository : IVoucherRepository
    {
        private readonly DBContext _context;

        public VoucherRepository(DBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Voucher>> GetAllAsync()
        {
            return await _context.Vouchers.ToListAsync();
        }

        public async Task<Voucher?> GetByIdAsync(int id)
        {
            return await _context.Vouchers.FindAsync(id);
        }

        public async Task<bool> IncrementUsageAsync(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null) return false;

            voucher.UsageCount = (voucher.UsageCount ?? 0) + 1;
            
            if (voucher.Quantity.HasValue && voucher.Quantity.Value > 0)
            {
                voucher.Quantity -= 1;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
