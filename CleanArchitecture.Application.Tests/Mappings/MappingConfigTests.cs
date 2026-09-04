using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Application.Tests.Helpers;
using CleanArchitecture.Domain.Entities;
using Mapster;

namespace CleanArchitecture.Application.Tests.Mappings;

public class MappingConfigTests
{
    [Fact]
    public void CreateCustomerDto_AdaptedOntoExistingCustomer_ShouldClearOptionalFields()
    {
        var customer = new Customer
        {
            Id = 7,
            Name = "Alice Martin",
            Email = "alice@example.test",
            Phone = "+1 555-0101",
            Company = "Northwind Traders",
            Notes = "Prefers email",
            IsActive = true,
            CreatedAt = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CreatedBy = "System"
        };

        var dto = new CreateCustomerDto
        {
            Name = "Alice Martin",
            Email = "alice@example.test",
            Phone = null,
            Company = null,
            Notes = null,
            IsActive = true
        };

        dto.Adapt(customer);

        customer.Phone.Should().BeNull();
        customer.Company.Should().BeNull();
        customer.Notes.Should().BeNull();
    }

    [Fact]
    public void CreateCustomerDto_AdaptedOntoExistingCustomer_ShouldPreserveIdentityAndAudit()
    {
        var createdAt = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var customer = new Customer
        {
            Id = 7,
            Name = "Alice Martin",
            Email = "alice@example.test",
            CreatedAt = createdAt,
            CreatedBy = "System"
        };

        new CreateCustomerDto { Name = "Alice Moreau", Email = "alice@example.test" }.Adapt(customer);

        customer.Id.Should().Be(7);
        customer.CreatedAt.Should().Be(createdAt);
        customer.CreatedBy.Should().Be("System");
        customer.Name.Should().Be("Alice Moreau");
    }

    [Fact]
    public void CreateProductDto_AdaptedOntoExistingProduct_ShouldClearOptionalFields()
    {
        var product = TestDataBuilder.CreateValidProduct();
        product.Description.Should().NotBeNull();
        product.Category.Should().NotBeNull();

        var dto = TestDataBuilder.CreateValidProductDto();
        dto.Description = null;
        dto.Category = null;

        dto.Adapt(product);

        product.Description.Should().BeNull();
        product.Category.Should().BeNull();
    }

    [Fact]
    public void Product_AdaptedToDto_ShouldProjectStockAndCustomerName()
    {
        var product = TestDataBuilder.CreateValidProduct();
        product.Customer = new Customer { Id = product.CustomerId, Name = "Northwind Traders", Email = "n@example.test" };

        var dto = product.Adapt<ProductDto>();

        dto.IsInStock.Should().BeTrue();
        dto.CustomerName.Should().Be("Northwind Traders");
    }

    [Fact]
    public void Product_WithoutLoadedCustomer_ShouldAdaptToEmptyCustomerName()
    {
        var product = TestDataBuilder.CreateValidProduct();

        product.Adapt<ProductDto>().CustomerName.Should().BeEmpty();
    }
}
