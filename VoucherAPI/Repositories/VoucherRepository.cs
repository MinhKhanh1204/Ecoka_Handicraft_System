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
    }
}
