using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Enums;
using CleanArchitecture.Application.DTOs;

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

    public static CreateUserDto CreateValidUserDto(string firstName = "Jane", string lastName = "Smith", string? email = null)
    {
        return new CreateUserDto
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email ?? $"{firstName.ToLower()}.{lastName.ToLower()}@example.com",
            PhoneNumber = "+1234567890",
            Role = UserRole.User,
            DateOfBirth = new DateTime(1985, 5, 10)
        };
    }

    public static CreateProductDto CreateValidProductDto(string name = "Test Product", int userId = 1)
    {
        return new CreateProductDto
        {
            Name = name,
            Description = "Test product description",
            Price = 99.99m,
            StockQuantity = 10,
            Category = "Test Category",
            UserId = userId
        };
    }

    public static User CreateInactiveUser(int id = 1)
    {
        var user = CreateValidUser(id);
        user.IsActive = false;
        return user;
    }

    public static User CreateAdminUser(int id = 1)
    {
        var user = CreateValidUser(id);
        user.Role = UserRole.Admin;
        user.FirstName = "Admin";
        user.LastName = "User";
        user.Email = $"admin.{id}@company.com";
        return user;
    }

    public static Product CreateInactiveProduct(int id = 1, int userId = 1)
    {
        var product = CreateValidProduct(id, userId);
        product.Status = ProductStatus.Inactive;
        product.IsAvailable = false;
        return product;
    }

    public static Product CreateOutOfStockProduct(int id = 1, int userId = 1)
    {
        var product = CreateValidProduct(id, userId);
        product.StockQuantity = 0;
        product.IsAvailable = false;
        return product;
    }

    public static List<Product> CreateMixedStatusProductList(int userId = 1)
    {
        return new List<Product>
        {
            CreateValidProduct(1, userId),
            CreateInactiveProduct(2, userId),
            CreateOutOfStockProduct(3, userId)
        };
    }

    public static CreateUserDto CreateInvalidUserDto(string invalidEmail = "invalid-email")
    {
        var dto = CreateValidUserDto();
        dto.Email = invalidEmail;
        return dto;
    }
}
