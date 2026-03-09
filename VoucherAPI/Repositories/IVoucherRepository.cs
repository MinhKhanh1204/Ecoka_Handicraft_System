using System.Collections.Generic;
using System.Threading.Tasks;
using VoucherAPI.Models;

namespace VoucherAPI.Repositories
{
    public interface IVoucherRepository
    {
        Task<IEnumerable<Voucher>> GetAllAsync();
        Task<Voucher?> GetByIdAsync(int id);
    }
}
