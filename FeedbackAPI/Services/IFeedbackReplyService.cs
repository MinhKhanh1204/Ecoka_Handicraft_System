using FeedbackAPI.DTOs;

namespace FeedbackAPI.Services
{
    public interface IFeedbackReplyService
    {
        Task<IEnumerable<FeedbackReplyReadDto>> GetByFeedbackIdAsync(int feedbackId);
        Task<FeedbackReplyReadDto?> GetByIdAsync(int replyId);
        Task<FeedbackReplyReadDto> CreateAsync(int feedbackId, FeedbackReplyCreateDto dto);
        Task<FeedbackReplyReadDto?> UpdateAsync(int replyId, FeedbackReplyUpdateDto dto);
        Task<bool> DeleteAsync(int replyId);
    }
}
