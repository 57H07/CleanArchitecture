using CleanArchitecture.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.Phone)
            .HasMaxLength(30);

        builder.Property(e => e.Company)
            .HasMaxLength(200);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(100);

        builder.Property(e => e.UpdatedBy)
            .HasMaxLength(100);

        builder.HasIndex(e => e.Email);

        // Seed data
        builder.HasData(
            new Customer
            {
                Id = 1,
                Name = "Alice Martin",
                Email = "alice.martin@northwind.example",
                Phone = "+1 555-0101",
                Company = "Northwind Traders",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Customer
            {
                Id = 2,
                Name = "Bruno Legrand",
                Email = "bruno.legrand@contoso.example",
                Phone = "+1 555-0102",
                Company = "Contoso Ltd",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Customer
            {
                Id = 3,
                Name = "Chloe Dubois",
                Email = "chloe.dubois@adventure-works.example",
                Phone = "+1 555-0103",
                Company = "Adventure Works",
                IsActive = false,
                Notes = "On hold pending contract renewal.",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            }
        );
    }
}
