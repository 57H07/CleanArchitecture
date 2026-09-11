using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Enums;
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

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(100);

        builder.Property(e => e.UpdatedBy)
            .HasMaxLength(100);

        // Covers the list's default sort and the category filter.
        builder.HasIndex(e => e.Name);
        builder.HasIndex(e => e.Category);
        builder.HasIndex(e => e.Status);

        // A product belongs to a customer; deleting a customer that still owns
        // products is refused rather than cascading the delete to them.
        builder.HasOne(e => e.Customer)
            .WithMany()
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed timestamps are fixed, not DateTime.UtcNow: HasData feeds the migration
        // snapshot, and a moving value makes every `dotnet ef migrations add` produce a
        // spurious diff.
        builder.HasData(
            new Product
            {
                Id = 1,
                Name = "Laptop Computer",
                Description = "High-performance laptop for professional use",
                Price = 1299.99m,
                StockQuantity = 50,
                Category = "Electronics",
                Status = ProductStatus.Active,
                CustomerId = 1,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new Product
            {
                Id = 2,
                Name = "Wireless Mouse",
                Description = "Ergonomic wireless mouse with long battery life",
                Price = 29.99m,
                StockQuantity = 100,
                Category = "Electronics",
                Status = ProductStatus.Active,
                CustomerId = 1,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new Product
            {
                Id = 3,
                Name = "Office Chair",
                Description = "Comfortable office chair with lumbar support",
                Price = 249.99m,
                StockQuantity = 25,
                Category = "Furniture",
                Status = ProductStatus.Draft,
                CustomerId = 2,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            }
        );
    }
}
