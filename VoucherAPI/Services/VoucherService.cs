using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using VoucherAPI.DTOs;
using VoucherAPI.Repositories;

namespace VoucherAPI.Services
{
    public class VoucherService : IVoucherService
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly IMapper _mapper;

        public VoucherService(IVoucherRepository voucherRepository, IMapper mapper)
        {
            _voucherRepository = voucherRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<VoucherDto>> GetAllVouchersAsync()
        {
            var vouchers = await _voucherRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<VoucherDto>>(vouchers);
        }

        public async Task<VoucherDto?> GetVoucherByIdAsync(int id)
        {
            var voucher = await _voucherRepository.GetByIdAsync(id);
            return _mapper.Map<VoucherDto>(voucher);
        }
    }
}
