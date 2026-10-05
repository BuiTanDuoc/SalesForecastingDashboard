using Microsoft.EntityFrameworkCore;
using SalesForecastingDashboard.Models;

namespace SalesForecastingDashboard.Data
{
    public class ProjectContext : DbContext
    {
        public ProjectContext(DbContextOptions<ProjectContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }

        public DbSet<Notification> Notifications { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---------- PRODUCT TABLOSU ----------
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("products"); // PostgreSQL tablo adı (küçük harf)

                entity.HasKey(p => p.Id);

                entity.Property(p => p.Id).HasColumnName("id");
                entity.Property(p => p.Name).HasColumnName("name");
                entity.Property(p => p.Stock).HasColumnName("stock");
                entity.Property(p => p.SafetyStock).HasColumnName("safety_stock");
                entity.Property(p => p.CreatedAt).HasColumnName("created_at");
            });

            // ---------- ORDER TABLOSU ----------
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("orders");

                entity.HasKey(o => o.Id);

                entity.Property(o => o.Id).HasColumnName("id");
                entity.Property(o => o.ProductId).HasColumnName("product_id");
                entity.Property(o => o.Quantity).HasColumnName("quantity");
                entity.Property(o => o.OrderDate).HasColumnName("order_date");

                entity.HasOne(o => o.Product)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(o => o.ProductId)
                    .HasConstraintName("fk_orders_products")
                    .OnDelete(DeleteBehavior.Cascade);
            });


            {
                modelBuilder.Entity<Notification>()
                .ToTable("notifications");
            }

        }
    }
}
