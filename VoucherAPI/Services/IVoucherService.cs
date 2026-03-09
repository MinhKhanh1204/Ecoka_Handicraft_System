using System.Collections.Generic;
using System.Threading.Tasks;
using VoucherAPI.DTOs;

namespace VoucherAPI.Services
{
    public interface IVoucherService
    {
        Task<IEnumerable<VoucherDto>> GetAllVouchersAsync();
        Task<VoucherDto?> GetVoucherByIdAsync(int id);
    }
}
