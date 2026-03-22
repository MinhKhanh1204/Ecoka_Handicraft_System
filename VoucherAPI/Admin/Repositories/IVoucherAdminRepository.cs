using VoucherAPI.Models;

namespace VoucherAPI.Admin.Repositories
{
    public interface IVoucherAdminRepository
    {
        IQueryable<Voucher> GetQueryable();
        Task<Voucher?> GetByIdAsync(int id);
        Task<Voucher?> GetByCodeAsync(string code, int? excludeId = null);
        Task<Voucher> AddAsync(Voucher voucher);
        Task UpdateAsync(Voucher voucher);
        Task DeleteAsync(Voucher voucher);
    }
}
