using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Enums;

namespace CleanArchitecture.Application.Tests.Helpers;

public static class TestDataBuilder
{
    public static User CreateValidUser(int id = 1)
    {
        return new User
        {
            Id = id,
            FirstName = "John",
            LastName = "Doe",
            Email = $"john.doe.{id}@example.com",
            PhoneNumber = "+1234567890",
            Role = UserRole.User,
            IsActive = true,
            DateOfBirth = new DateTime(1990, 1, 15),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "Test"
        };
    }

    public static Product CreateValidProduct(int id = 1, int userId = 1)
    {
        return new Product
        {
            Id = id,
            Name = $"Test Product {id}",
            Description = "Test product description",
            Price = 99.99m,
            StockQuantity = 10,
            Category = "Test Category",
            Status = ProductStatus.Active,
            IsAvailable = true,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "Test"
        };
    }

    public static List<User> CreateUserList(int count = 3)
    {
        var users = new List<User>();
        for (int i = 1; i <= count; i++)
        {
            users.Add(CreateValidUser(i));
        }
        return users;
    }

    public static List<Product> CreateProductList(int count = 3, int userId = 1)
    {
        var products = new List<Product>();
        for (int i = 1; i <= count; i++)
        {
            products.Add(CreateValidProduct(i, userId));
        }
        return products;
    }
}
