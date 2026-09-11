using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Application.Tests.Helpers;
using CleanArchitecture.Domain.Enums;
using Mapster;

namespace CleanArchitecture.Application.Tests.Mappings;

public class ProductStatusMappingTests
{
    [Theory]
    [InlineData(ProductStatus.Active, 10, true)]
    [InlineData(ProductStatus.Active, 0, false)]
    [InlineData(ProductStatus.Draft, 10, false)]
    [InlineData(ProductStatus.Inactive, 10, false)]
    [InlineData(ProductStatus.Discontinued, 10, false)]
    public void Product_AdaptedToDto_ShouldMarkOnlyPublishedStockAsPurchasable(
        ProductStatus status,
        int stockQuantity,
        bool expectedPurchasable)
    {
        var product = TestDataBuilder.CreateValidProduct();
        product.Status = status;
        product.StockQuantity = stockQuantity;

        var dto = product.Adapt<ProductDto>();

        dto.IsPurchasable.Should().Be(expectedPurchasable);
        dto.Status.Should().Be(status);
    }

    [Fact]
    public void Product_InStockButUnpublished_ShouldStillReportStockOnHand()
    {
        var product = TestDataBuilder.CreateValidProduct();
        product.Status = ProductStatus.Draft;
        product.StockQuantity = 25;

        var dto = product.Adapt<ProductDto>();

        // The regression: stock is a fact about the warehouse, not about publication.
        dto.IsInStock.Should().BeTrue();
        dto.IsPurchasable.Should().BeFalse();
    }

    [Fact]
    public void ProductDto_AdaptedToCreateDto_ShouldCarryStatusIntoTheEditForm()
    {
        var dto = new ProductDto
        {
            Id = 4,
            Name = "Standing Desk",
            Price = 649.50m,
            StockQuantity = 3,
            Category = "Furniture",
            Status = ProductStatus.Inactive,
            CustomerId = 2
        };

        var editDto = dto.Adapt<CreateProductDto>();

        editDto.Status.Should().Be(ProductStatus.Inactive);
        editDto.CustomerId.Should().Be(2);
        editDto.Name.Should().Be("Standing Desk");
    }
}
