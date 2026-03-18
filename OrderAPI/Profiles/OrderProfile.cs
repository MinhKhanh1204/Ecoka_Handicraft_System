using AutoMapper;
using OrderAPI.DTOs;
using OrderAPI.Models;

namespace OrderAPI.Profiles
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            // Ensure nested OrderItems are explicitly mapped so DTO always carries items
            CreateMap<Order, OrderReadDto>()
                .ForMember(dest => dest.OrderItems,
                           opt => opt.MapFrom(src => src.OrderItems));

            CreateMap<OrderCreateDto, Order>()
                .ForMember(dest => dest.OrderID, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<OrderUpdateDto, Order>()
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember) => srcMember != null));

            // OrderItem mapping
            CreateMap<OrderItem, OrderItemReadDto>();

            CreateMap<OrderItemCreateDto, OrderItem>()
                .ForMember(dest => dest.OrderItemID, opt => opt.Ignore())
                .ForMember(dest => dest.OrderID, opt => opt.Ignore());

            CreateMap<OrderItemUpdateDto, OrderItem>()
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
