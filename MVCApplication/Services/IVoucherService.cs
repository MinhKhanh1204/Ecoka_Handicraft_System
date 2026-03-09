using MVCApplication.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MVCApplication.Services
{
    public interface IVoucherService
    {
        Task<List<VoucherDto>> GetAllVouchersAsync();
        Task<VoucherDto?> GetVoucherByIdAsync(int id);
    }
}
