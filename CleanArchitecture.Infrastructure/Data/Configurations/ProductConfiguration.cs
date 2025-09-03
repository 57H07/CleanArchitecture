using CleanArchitecture.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(e => e.Description)
            .HasMaxLength(1000);
            
        builder.Property(e => e.Price)
            .IsRequired()
            .HasPrecision(18, 2);
            
        builder.Property(e => e.StockQuantity)
            .IsRequired();
            
        builder.Property(e => e.Category)
            .HasMaxLength(100);
            
        builder.Property(e => e.CreatedAt)
            .IsRequired();
            
        builder.Property(e => e.CreatedBy)
            .HasMaxLength(100);
            
        builder.Property(e => e.UpdatedBy)
            .HasMaxLength(100);

        // Seed data
        builder.HasData(
            new Product
            {
                Id = 1,
                Name = "Laptop Computer",
                Description = "High-performance laptop for professional use",
                Price = 1299.99m,
                StockQuantity = 50,
                Category = "Electronics",
                IsAvailable = true,
                UserId = 1,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Product
            {
                Id = 2,
                Name = "Wireless Mouse",
                Description = "Ergonomic wireless mouse with long battery life",
                Price = 29.99m,
                StockQuantity = 100,
                Category = "Electronics",
                IsAvailable = true,
                UserId = 1,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Product
            {
                Id = 3,
                Name = "Office Chair",
                Description = "Comfortable office chair with lumbar support",
                Price = 249.99m,
                StockQuantity = 25,
                Category = "Furniture",
                IsAvailable = true,
                UserId = 2,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            }
        );
    }
}
