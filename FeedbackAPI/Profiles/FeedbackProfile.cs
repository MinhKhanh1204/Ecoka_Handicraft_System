using AutoMapper;
using FeedbackAPI.DTOs;
using FeedbackAPI.Models;

namespace FeedbackAPI.Profiles
{
    public class FeedbackProfile : Profile
    {
        public FeedbackProfile()
        {
            // ========================
            // Feedback Mapping
            // ========================

            // Entity -> ReadDto
            CreateMap<Feedback, FeedbackReadDto>()
                .ForMember(dest => dest.Replies, opt => opt.MapFrom(src => src.Replies));

            // CreateDto -> Entity
            CreateMap<FeedbackCreateDto, Feedback>()
                .ForMember(dest => dest.FeedbackID, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Replies, opt => opt.Ignore());

            // UpdateDto -> Entity (only map non-null values)
            CreateMap<FeedbackUpdateDto, Feedback>()
                .ForMember(dest => dest.FeedbackID, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerID, opt => opt.Ignore())
                .ForMember(dest => dest.ProductID, opt => opt.Ignore())
                .ForMember(dest => dest.OrderID, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Replies, opt => opt.Ignore())
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember) => srcMember != null));

            // ========================
            // FeedbackReply Mapping
            // ========================

            // Entity -> ReadDto
            CreateMap<FeedbackReply, FeedbackReplyReadDto>();

            // CreateDto -> Entity
            CreateMap<FeedbackReplyCreateDto, FeedbackReply>()
                .ForMember(dest => dest.ReplyID, opt => opt.Ignore())
                .ForMember(dest => dest.FeedbackID, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Feedback, opt => opt.Ignore());

            // UpdateDto -> Entity (only map non-null values)
            CreateMap<FeedbackReplyUpdateDto, FeedbackReply>()
                .ForMember(dest => dest.ReplyID, opt => opt.Ignore())
                .ForMember(dest => dest.FeedbackID, opt => opt.Ignore())
                .ForMember(dest => dest.StaffID, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Feedback, opt => opt.Ignore())
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
