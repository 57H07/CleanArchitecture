using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Enums;
using CleanArchitecture.Application.DTOs;

namespace CleanArchitecture.Application.Tests.Helpers;

public static class TestDataBuilder
{

    public static Product CreateValidProduct(int id = 1, int customerId = 1)
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
            CustomerId = customerId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "Test"
        };
    }

    public static List<Product> CreateProductList(int count = 3, int customerId = 1)
    {
        var products = new List<Product>();
        for (int i = 1; i <= count; i++)
        {
            products.Add(CreateValidProduct(i, customerId));
        }
        return products;
    }

    public static CreateProductDto CreateValidProductDto(string name = "Test Product", int customerId = 1)
    {
        return new CreateProductDto
        {
            Name = name,
            Description = "Test product description",
            Price = 99.99m,
            StockQuantity = 10,
            Category = "Test Category",
            CustomerId = customerId
        };
    }

    public static Product CreateInactiveProduct(int id = 1, int customerId = 1)
    {
        var product = CreateValidProduct(id, customerId);
        product.Status = ProductStatus.Inactive;
        product.IsAvailable = false;
        return product;
    }

    public static Product CreateOutOfStockProduct(int id = 1, int customerId = 1)
    {
        var product = CreateValidProduct(id, customerId);
        product.StockQuantity = 0;
        product.IsAvailable = false;
        return product;
    }

    public static List<Product> CreateMixedStatusProductList(int customerId = 1)
    {
        return new List<Product>
        {
            CreateValidProduct(1, customerId),
            CreateInactiveProduct(2, customerId),
            CreateOutOfStockProduct(3, customerId)
        };
    }

    public static Customer CreateValidCustomer(int id = 1)
    {
        return new Customer
        {
            Id = id,
            Name = $"Test Customer {id}",
            Email = $"customer.{id}@example.com",
            Phone = "+1234567890",
            Company = "Test Company",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "Test"
        };
    }

    public static List<Customer> CreateCustomerList(int count = 3)
    {
        var customers = new List<Customer>();
        for (int i = 1; i <= count; i++)
        {
            customers.Add(CreateValidCustomer(i));
        }
        return customers;
    }

    public static CreateCustomerDto CreateValidCustomerDto(string name = "New Customer", string? email = null)
    {
        return new CreateCustomerDto
        {
            Name = name,
            Email = email ?? $"{name.Replace(" ", ".").ToLower()}@example.com",
            Phone = "+1234567890",
            Company = "Test Company",
            IsActive = true
        };
    }
}
