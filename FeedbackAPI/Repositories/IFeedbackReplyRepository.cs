using FeedbackAPI.Models;

namespace FeedbackAPI.Repositories
{
    public interface IFeedbackReplyRepository
    {
        Task<IEnumerable<FeedbackReply>> GetByFeedbackIdAsync(int feedbackId);
        Task<FeedbackReply?> GetByIdAsync(int replyId);
        Task<FeedbackReply> CreateAsync(int feedbackId, FeedbackReply reply);
        Task<FeedbackReply?> UpdateAsync(int replyId, FeedbackReply updatedData);
        Task<bool> DeleteAsync(int replyId);
        Task DeleteEntityAsync(FeedbackReply reply);
    }
}
