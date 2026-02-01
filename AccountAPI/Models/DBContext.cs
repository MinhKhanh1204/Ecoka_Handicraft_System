using Microsoft.EntityFrameworkCore;

namespace AccountAPI.Models
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options)
            : base(options) { }
        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserRole>()
                .HasKey(x => new { x.AccountID, x.RoleID });

            modelBuilder.Entity<UserRole>()
                .HasOne(x => x.Account)
                .WithMany(a => a.UserRoles)
                .HasForeignKey(x => x.AccountID);

            modelBuilder.Entity<UserRole>()
                .HasOne(x => x.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(x => x.RoleID);
        }
    }
}
