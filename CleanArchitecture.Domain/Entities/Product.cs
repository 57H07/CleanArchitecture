using CleanArchitecture.Domain.Common;
using CleanArchitecture.Domain.Enums;
using CleanArchitecture.Domain.Exceptions;

namespace CleanArchitecture.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public string? Category { get; set; }

    public ProductStatus Status { get; set; } = ProductStatus.Draft;

    public int CustomerId { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public bool IsInStock() => StockQuantity > 0;

    public bool IsPublished() => Status == ProductStatus.Active;

    public bool IsDraft() => Status == ProductStatus.Draft;

    public bool IsDiscontinued() => Status == ProductStatus.Discontinued;

    public bool IsPurchasable() => IsPublished() && IsInStock();

    public void UpdateStock(int quantity)
    {
        if (StockQuantity + quantity < 0)
            throw new InsufficientStockException();

        StockQuantity += quantity;
    }

    public void SetPrice(decimal newPrice)
    {
        if (newPrice <= 0)
            throw new InvalidPriceException();

        Price = newPrice;
    }

    public void Publish()
    {
        if (Status is ProductStatus.Draft or ProductStatus.Inactive)
            Status = ProductStatus.Active;
    }

    public void Deactivate()
    {
        if (Status == ProductStatus.Active)
            Status = ProductStatus.Inactive;
    }

    public void Discontinue() => Status = ProductStatus.Discontinued;

    public bool HasValidName() => !string.IsNullOrWhiteSpace(Name) && Name.Length <= 200;

    public bool HasValidPrice() => Price > 0;

    public bool HasValidStock() => StockQuantity >= 0;

    public bool HasValidDescription() => Description == null || Description.Length <= 1000;

    public bool HasValidCategory() => Category == null || Category.Length <= 100;

    public void ValidateBusinessRules()
    {
        if (!HasValidName())
            throw new ValidationDomaineException("Product name is required and cannot exceed 200 characters", "Name");

        if (!HasValidPrice())
            throw new InvalidPriceException();

        if (!HasValidStock())
            throw new ValidationDomaineException("Stock quantity cannot be negative", "StockQuantity");

        if (!HasValidDescription())
            throw new ValidationDomaineException("Description cannot exceed 1000 characters", "Description");

        if (!HasValidCategory())
            throw new ValidationDomaineException("Category cannot exceed 100 characters", "Category");
    }
}
