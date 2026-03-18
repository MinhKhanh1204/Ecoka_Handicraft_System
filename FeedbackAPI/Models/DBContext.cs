using Microsoft.EntityFrameworkCore;

namespace FeedbackAPI.Models
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options) { }

        public DbSet<Feedback> Feedbacks => Set<Feedback>();
        public DbSet<FeedbackImage> FeedbackImages => Set<FeedbackImage>();
        
    }
}
