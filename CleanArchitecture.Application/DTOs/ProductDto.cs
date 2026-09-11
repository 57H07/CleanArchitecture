using System.ComponentModel.DataAnnotations;
using CleanArchitecture.Domain.Enums;

namespace CleanArchitecture.Application.DTOs;

public class ProductDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Display(Name = "Price")]
    [DisplayFormat(DataFormatString = "{0:C}")]
    public decimal Price { get; set; }

    [Display(Name = "Stock Quantity")]
    public int StockQuantity { get; set; }

    public string? Category { get; set; }

    [Display(Name = "Status")]
    public ProductStatus Status { get; set; }

    [Display(Name = "In Stock")]
    public bool IsInStock { get; set; }

    [Display(Name = "Purchasable")]
    public bool IsPurchasable { get; set; }

    [Display(Name = "Customer")]
    public int CustomerId { get; set; }

    [Display(Name = "Customer")]
    public string CustomerName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
