using AutoMapper;
using CartAPI.DTOs;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CartAPI.Profiles
{
    public class CartProfile : Profile
    {
        public CartProfile()
        {
            // Cart <-> CartReadDto
            CreateMap<Cart, CartReadDto>();
            CreateMap<CartItem, CartItemReadDto>();

            // CartItemCreateDto -> CartItem
            CreateMap<CartItemCreateDto, CartItem>()
                .ForMember(dest => dest.CartItemID, opt => opt.Ignore())
                .ForMember(dest => dest.CartId, opt => opt.Ignore())
                .ForMember(dest => dest.Cart, opt => opt.Ignore());

            // CartItemUpdateDto -> CartItem
            CreateMap<CartItemUpdateDto, CartItem>()
                .ForMember(dest => dest.CartItemID, opt => opt.Ignore())
                .ForMember(dest => dest.CartId, opt => opt.Ignore())
                .ForMember(dest => dest.Cart, opt => opt.Ignore());
        }
    }
}
