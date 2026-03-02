using Microsoft.EntityFrameworkCore;

namespace FeedbackAPI.Models
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options) { }

        public DbSet<Feedback> Feedbacks => Set<Feedback>();
        public DbSet<FeedbackReply> FeedbackReplies => Set<FeedbackReply>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FeedbackReply>()
                .HasOne(r => r.Feedback)
                .WithMany(f => f.Replies)
                .HasForeignKey(r => r.FeedbackID)
                .HasPrincipalKey(f => f.FeedbackID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
