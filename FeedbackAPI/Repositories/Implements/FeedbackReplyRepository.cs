using Microsoft.EntityFrameworkCore;
using FeedbackAPI.Models;

namespace FeedbackAPI.Repositories.Implements
{
    public class FeedbackReplyRepository : IFeedbackReplyRepository
    {
        private readonly DBContext _context;

        public FeedbackReplyRepository(DBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FeedbackReply>> GetByFeedbackIdAsync(int feedbackId)
        {
            return await _context.FeedbackReplies
                .Where(r => r.FeedbackID == feedbackId)
                .OrderByDescending(r => r.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<FeedbackReply?> GetByIdAsync(int replyId)
        {
            return await _context.FeedbackReplies
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ReplyID == replyId);
        }

        public async Task<FeedbackReply> CreateAsync(int feedbackId, FeedbackReply reply)
        {
            reply.FeedbackID = feedbackId;
            reply.CreatedAt = DateTime.UtcNow;
            reply.UpdatedAt = null;

            _context.FeedbackReplies.Add(reply);
            await _context.SaveChangesAsync();
            return reply;
        }

        public async Task<FeedbackReply?> UpdateAsync(int replyId, FeedbackReply updatedData)
        {
            var reply = await _context.FeedbackReplies.FirstOrDefaultAsync(r => r.ReplyID == replyId);
            if (reply == null)
                return null;

            if (!string.IsNullOrWhiteSpace(updatedData.ReplyContent))
                reply.ReplyContent = updatedData.ReplyContent;

            reply.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return reply;
        }

        public async Task<bool> DeleteAsync(int replyId)
        {
            var reply = await _context.FeedbackReplies.FirstOrDefaultAsync(r => r.ReplyID == replyId);
            if (reply == null)
                return false;

            _context.FeedbackReplies.Remove(reply);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task DeleteEntityAsync(FeedbackReply reply)
        {
            _context.FeedbackReplies.Attach(reply);
            _context.FeedbackReplies.Remove(reply);
            await _context.SaveChangesAsync();
        }
    }
}
