using Microsoft.EntityFrameworkCore;
using FeedbackAPI.Models;

namespace FeedbackAPI.Repositories.Implements
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly DBContext _context;

        public FeedbackRepository(DBContext context)
        {
            _context = context;
        }
        public async Task<bool> ExistsAsync(string customerId, string productId)
        {
            return await _context.Feedbacks
                .AnyAsync(f =>
                    f.CustomerID == customerId &&
                    f.ProductID == productId &&
                    f.Status == "Active");
        }
        // ================= READ =================

        public async Task<IEnumerable<Feedback>> GetAllAsync()
        {
            return await _context.Feedbacks
                .OrderByDescending(f => f.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Feedback?> GetByIdAsync(int feedbackId)
        {
            return await _context.Feedbacks
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.FeedbackID == feedbackId);
        }

        // ================= FILTER =================

        public async Task<IEnumerable<Feedback>> FilterAsync(
    string? customerId,
    string? productId,
    int? minRating,
    int? maxRating,
    string? status,
    DateTime? from,
    DateTime? to)
        {
            var query = _context.Feedbacks.AsQueryable();

            if (!string.IsNullOrWhiteSpace(customerId))
                query = query.Where(f => f.CustomerID == customerId);

            if (!string.IsNullOrWhiteSpace(productId))
                query = query.Where(f => f.ProductID == productId);

            if (minRating.HasValue)
                query = query.Where(f => f.Rating >= minRating.Value);

            if (maxRating.HasValue)
                query = query.Where(f => f.Rating <= maxRating.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(f => f.Status == status);

            if (from.HasValue)
                query = query.Where(f => f.CreatedAt >= from.Value);

            if (to.HasValue)
                query = query.Where(f => f.CreatedAt <= to.Value);

            return await query
                .OrderByDescending(f => f.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        // ================= CREATE =================

        public async Task<Feedback> CreateAsync(Feedback feedback)
        {
            feedback.CreatedAt = DateTime.UtcNow;
            feedback.UpdatedAt = null;

            if (string.IsNullOrWhiteSpace(feedback.Status))
                feedback.Status = "Active";

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();
            return feedback;
        }

        // ================= UPDATE =================

        public async Task<Feedback?> UpdateAsync(int feedbackId, Feedback updatedData)
        {
            var feedback = await _context.Feedbacks.FirstOrDefaultAsync(f => f.FeedbackID == feedbackId);
            if (feedback == null)
                return null;

            if (updatedData.Rating >= 1 && updatedData.Rating <= 5)
                feedback.Rating = updatedData.Rating;

            if (updatedData.Comment != null)
                feedback.Comment = updatedData.Comment;

            if (!string.IsNullOrWhiteSpace(updatedData.Status))
                feedback.Status = updatedData.Status;

            feedback.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return feedback;
        }

        // ================= DELETE =================

        public async Task<bool> DeleteAsync(int feedbackId)
        {
            var feedback = await _context.Feedbacks.FirstOrDefaultAsync(f => f.FeedbackID == feedbackId);
            if (feedback == null)
                return false;

            feedback.Status = "Deleted";
            feedback.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task DeleteEntityAsync(Feedback feedback)
        {
            _context.Feedbacks.Attach(feedback);
            feedback.Status = "Deleted";
            feedback.UpdatedAt = DateTime.UtcNow;
            _context.Entry(feedback).Property(f => f.Status).IsModified = true;
            _context.Entry(feedback).Property(f => f.UpdatedAt).IsModified = true;
            await _context.SaveChangesAsync();
        }
    }
}
