using FeedbackAPI.DTOs;

namespace FeedbackAPI.Services
{
    public interface IFeedbackService
    {
        // ===== READ =====
        Task<IEnumerable<FeedbackReadDto>> GetAllAsync();
        Task<FeedbackReadDto?> GetByIdAsync(int feedbackId);

        // ===== FILTER =====
        Task<IEnumerable<FeedbackReadDto>> FilterAsync(FeedbackFilterDto filter);

        // ===== CREATE =====
        Task<FeedbackReadDto> CreateAsync(FeedbackCreateDto dto);

        // ===== UPDATE =====
        Task<FeedbackReadDto?> UpdateAsync(int feedbackId, FeedbackUpdateDto dto);

        // ===== DELETE =====
        Task<bool> DeleteAsync(int feedbackId);
    }
}
