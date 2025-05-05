using Microsoft.EntityFrameworkCore;
using POS.Core.Models;

namespace POS.Data
{
    public class POSDbContext : DbContext
    {
        public POSDbContext(DbContextOptions<POSDbContext> options) : base(options) { }
        
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Tax> Taxes { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuration des relations et contraintes
            modelBuilder.Entity<SaleItem>()
                .HasOne(si => si.Product)
                .WithMany()
                .HasForeignKey(si => si.ProductId);
                
            modelBuilder.Entity<Sale>()
                .HasMany(s => s.Items)
                .WithOne()
                .HasForeignKey(si => si.SaleId);
                
            // Configuration spécifique au Maroc
            modelBuilder.Entity<Tax>().HasData(
                new Tax { Id = 1, Name = "TVA Standard", Rate = 20m },
                new Tax { Id = 2, Name = "TVA Réduite", Rate = 10m },
                new Tax { Id = 3, Name = "TVA Super Réduite", Rate = 7m }
            );
        }
    }
}