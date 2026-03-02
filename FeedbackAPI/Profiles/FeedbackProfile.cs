using AutoMapper;
using FeedbackAPI.DTOs;
using FeedbackAPI.Models;

namespace FeedbackAPI.Profiles
{
    public class FeedbackProfile : Profile
    {
        public FeedbackProfile()
        {
            // Entity -> ReadDto
            CreateMap<Feedback, FeedbackReadDto>();

            // CreateDto -> Entity
            CreateMap<FeedbackCreateDto, Feedback>()
                .ForMember(dest => dest.FeedbackID, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore());

            // UpdateDto -> Entity (only map non-null values)
            CreateMap<FeedbackUpdateDto, Feedback>()
                .ForMember(dest => dest.FeedbackID, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerID, opt => opt.Ignore())
                .ForMember(dest => dest.ProductID, opt => opt.Ignore())
                .ForMember(dest => dest.OrderID, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
