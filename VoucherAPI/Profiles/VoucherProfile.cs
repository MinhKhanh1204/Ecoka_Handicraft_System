using AutoMapper;
using VoucherAPI.DTOs;
using VoucherAPI.Models;

namespace VoucherAPI.Profiles
{
    public class VoucherProfile : Profile
    {
        public VoucherProfile()
        {
            CreateMap<Voucher, VoucherDto>();
        }
    }
}
