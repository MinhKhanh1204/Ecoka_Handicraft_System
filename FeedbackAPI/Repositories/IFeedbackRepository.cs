using FeedbackAPI.Models;

namespace FeedbackAPI.Repositories
{
    public interface IFeedbackRepository
    {
        // ===== READ =====
        Task<IEnumerable<Feedback>> GetAllAsync();
        Task<Feedback?> GetByIdAsync(int feedbackId);

        // ===== FILTER =====
        Task<IEnumerable<Feedback>> FilterAsync(
            int? customerId,
            int? productId,
            int? minRating,
            int? maxRating,
            string? status,
            DateTime? from,
            DateTime? to);

        // ===== CREATE =====
        Task<Feedback> CreateAsync(Feedback feedback);

        // ===== UPDATE =====
        Task<Feedback?> UpdateAsync(int feedbackId, Feedback updatedData);

        // ===== DELETE =====
        Task<bool> DeleteAsync(int feedbackId);
        Task DeleteEntityAsync(Feedback feedback);
    }
}
