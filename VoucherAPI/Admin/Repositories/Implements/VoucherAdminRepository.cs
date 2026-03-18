using Microsoft.EntityFrameworkCore;
using VoucherAPI.Models;

namespace VoucherAPI.Admin.Repositories.Implements
{
    public class VoucherAdminRepository : IVoucherAdminRepository
    {
        private readonly DBContext _context;

        public VoucherAdminRepository(DBContext context)
        {
            _context = context;
        }

        public async Task<IQueryable<Voucher>> GetQueryableAsync()
        {
            return await Task.FromResult(_context.Vouchers.AsNoTracking().AsQueryable());
        }

        public async Task<Voucher?> GetByIdAsync(int id)
        {
            return await _context.Vouchers.FindAsync(id);
        }

        public async Task<Voucher?> GetByCodeAsync(string code, int? excludeId = null)
        {
            var query = _context.Vouchers.Where(v => v.Code == code);
            if (excludeId.HasValue)
                query = query.Where(v => v.VoucherId != excludeId.Value);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<Voucher> AddAsync(Voucher voucher)
        {
            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();
            return voucher;
        }

        public async Task UpdateAsync(Voucher voucher)
        {
            _context.Vouchers.Update(voucher);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Voucher voucher)
        {
            _context.Vouchers.Remove(voucher);
            await _context.SaveChangesAsync();
        }
    }
}
