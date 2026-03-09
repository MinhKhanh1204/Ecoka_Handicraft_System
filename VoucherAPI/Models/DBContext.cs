using Microsoft.EntityFrameworkCore;
namespace VoucherAPI.Models
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {
        }
        public DbSet<Voucher> Vouchers { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Voucher>(entity =>
            {
                entity.HasKey(e => e.VoucherId);
                entity.Property(e => e.Code).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(255);
                entity.Property(e => e.DiscountPercentage).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.MaxReducing).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.MinOrderValue).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.UsageCount).HasDefaultValue(0);
                entity.Property(e => e.VoucherName).HasMaxLength(100);
            });
        }
    }
}
