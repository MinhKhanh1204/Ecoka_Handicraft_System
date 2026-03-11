using VoucherAPI.Models;

namespace VoucherAPI.Admin.Repositories
{
    public interface IVoucherAdminRepository
    {
        Task<IQueryable<Voucher>> GetQueryableAsync();
        Task<Voucher?> GetByIdAsync(int id);
        Task<Voucher?> GetByCodeAsync(string code, int? excludeId = null);
        Task<Voucher> AddAsync(Voucher voucher);
        Task UpdateAsync(Voucher voucher);
        Task DeleteAsync(Voucher voucher);
    }
}
