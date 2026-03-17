using AccountAPI.DTOs;
using AutoMapper;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace AccountAPI.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // login response
            CreateMap<string, LoginResponseDto>()
                .ForMember(dest => dest.AccessToken,
                           opt => opt.MapFrom(src => src));
        }
    }
}
