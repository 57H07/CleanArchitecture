using CleanArchitecture.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(256);
            
        builder.HasIndex(e => e.Email)
            .IsUnique();
            
        builder.Property(e => e.PhoneNumber)
            .HasMaxLength(20);
            
        builder.Property(e => e.CreatedAt)
            .IsRequired();
            
        builder.Property(e => e.CreatedBy)
            .HasMaxLength(100);
            
        builder.Property(e => e.UpdatedBy)
            .HasMaxLength(100);

        // Configure relationship
        builder.HasMany(e => e.Products)
            .WithOne(e => e.User)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed data
        builder.HasData(
            new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "+1234567890",
                IsActive = true,
                DateOfBirth = new DateTime(1990, 1, 15),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new User
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                PhoneNumber = "+1987654321",
                IsActive = true,
                DateOfBirth = new DateTime(1985, 6, 20),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            }
        );
    }
}
