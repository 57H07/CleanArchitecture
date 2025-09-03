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
    
    public bool IsAvailable { get; set; } = true;
    
    // Foreign key
    public int UserId { get; set; }
    
    // Navigation property
    public virtual User User { get; set; } = null!;
    
    // Domain methods
    public bool IsInStock() => StockQuantity > 0 && Status == ProductStatus.Active;
    
    public bool IsPublished() => Status == ProductStatus.Active;
    
    public bool IsDraft() => Status == ProductStatus.Draft;
    
    public bool IsDiscontinued() => Status == ProductStatus.Discontinued;
    
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
        if (Status == ProductStatus.Draft)
        {
            Status = ProductStatus.Active;
            IsAvailable = true;
        }
    }
    
    public void Deactivate()
    {
        if (Status == ProductStatus.Active)
        {
            Status = ProductStatus.Inactive;
            IsAvailable = false;
        }
    }
    
    public void Reactivate()
    {
        if (Status == ProductStatus.Inactive)
        {
            Status = ProductStatus.Active;
            IsAvailable = true;
        }
    }
    
    public void Discontinue()
    {
        Status = ProductStatus.Discontinued;
        IsAvailable = false;
    }
    
    // Domain validation methods
    public bool HasValidName()
    {
        return !string.IsNullOrWhiteSpace(Name) && Name.Length <= 200;
    }
    
    public bool HasValidPrice()
    {
        return Price > 0;
    }
    
    public bool HasValidStock()
    {
        return StockQuantity >= 0;
    }
    
    public bool HasValidDescription()
    {
        return Description == null || Description.Length <= 1000;
    }
    
    public bool HasValidCategory()
    {
        return Category == null || Category.Length <= 100;
    }
    
    public void ValidateBusinessRules()
    {
        if (!HasValidName())
            throw new ArgumentException("Product name is required and cannot exceed 200 characters");
            
        if (!HasValidPrice())
            throw new ArgumentException("Product price must be greater than 0");
            
        if (!HasValidStock())
            throw new ArgumentException("Stock quantity cannot be negative");
            
        if (!HasValidDescription())
            throw new ArgumentException("Description cannot exceed 1000 characters");
            
        if (!HasValidCategory())
            throw new ArgumentException("Category cannot exceed 100 characters");
    }
}
