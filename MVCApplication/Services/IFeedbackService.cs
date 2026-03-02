using MVCApplication.Models;

namespace MVCApplication.Services
{
    public interface IFeedbackService
    {
        Task<IEnumerable<Feedback>> GetAllAsync();
        Task<Feedback?> GetByIdAsync(int feedbackId);
        Task<IEnumerable<Feedback>> FilterAsync(FeedbackFilterDto filter);
        Task<Feedback> CreateAsync(FeedbackCreateDto dto);
        Task<Feedback?> UpdateAsync(int feedbackId, FeedbackUpdateDto dto);
        Task<bool> DeleteAsync(int feedbackId);
    }
}
