using System.ComponentModel.DataAnnotations;
using CleanArchitecture.Domain.Common;

namespace CleanArchitecture.Domain.Entities;

public class Product : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }
    
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
    public int StockQuantity { get; set; }
    
    [MaxLength(100)]
    public string? Category { get; set; }
    
    public bool IsAvailable { get; set; } = true;
    
    // Foreign key
    public int UserId { get; set; }
    
    // Navigation property
    public virtual User User { get; set; } = null!;
    
    // Domain methods
    public bool IsInStock() => StockQuantity > 0;
    
    public void UpdateStock(int quantity)
    {
        if (StockQuantity + quantity < 0)
            throw new InvalidOperationException("Insufficient stock");
            
        StockQuantity += quantity;
    }
    
    public void SetPrice(decimal newPrice)
    {
        if (newPrice <= 0)
            throw new ArgumentException("Price must be greater than zero", nameof(newPrice));
            
        Price = newPrice;
    }
}
