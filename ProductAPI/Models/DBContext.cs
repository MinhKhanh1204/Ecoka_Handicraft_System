using Microsoft.EntityFrameworkCore;

namespace ProductAPI.Models
{
	public class DBContext : DbContext
	{
		public DBContext(DbContextOptions<DBContext> options)
			: base(options) { }

		public DbSet<Product> Products => Set<Product>();
		public DbSet<Category> Categories => Set<Category>();
		public DbSet<ProductImage> ProductImages => Set<ProductImage>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Product>()
				.HasOne(p => p.Category)
				.WithMany(c => c.Products)
				.HasForeignKey(p => p.CategoryID);

			modelBuilder.Entity<ProductImage>()
				.HasOne(i => i.Product)
				.WithMany(p => p.ProductImages)
				.HasForeignKey(i => i.ProductID);
		}
	}
}
