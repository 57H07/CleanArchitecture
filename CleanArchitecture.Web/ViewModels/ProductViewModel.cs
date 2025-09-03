using System.ComponentModel.DataAnnotations;
using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CleanArchitecture.Web.ViewModels;

/// <summary>
/// ViewModel specifically designed for the Product Create/Edit view
/// Contains UI-specific properties and logic
/// </summary>
public class ProductViewModel
{
    // Core product data (often maps from DTO)
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
    [Display(Name = "Product Name")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    [Display(Name = "Description")]
    public string? Description { get; set; }
    
    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    [Display(Name = "Price ($)")]
    [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
    public decimal Price { get; set; }
    
    [Required(ErrorMessage = "Stock quantity is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
    [Display(Name = "Stock Quantity")]
    public int StockQuantity { get; set; }
    
    [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
    [Display(Name = "Category")]
    public string? Category { get; set; }
    
    [Required(ErrorMessage = "Status is required")]
    [Display(Name = "Product Status")]
    public ProductStatus Status { get; set; }
    
    [Required(ErrorMessage = "User is required")]
    [Display(Name = "Assigned User")]
    public int UserId { get; set; }
    
    // UI-specific properties that don't exist in DTOs
    [Display(Name = "Available Users")]
    public SelectList? AvailableUsers { get; set; }
    
    [Display(Name = "Available Categories")]
    public SelectList? AvailableCategories { get; set; }
    
    [Display(Name = "Available Statuses")]
    public SelectList? AvailableStatuses { get; set; }
    
    // View-specific computed properties
    public string FormattedPrice => Price.ToString("C");
    public string StockStatus => StockQuantity > 0 ? "In Stock" : "Out of Stock";
    public string StatusBadgeClass => Status switch
    {
        ProductStatus.Active => "badge bg-success",
        ProductStatus.Draft => "badge bg-secondary",
        ProductStatus.Inactive => "badge bg-warning",
        ProductStatus.Discontinued => "badge bg-danger",
        _ => "badge bg-secondary"
    };
    
    // UI state properties
    public bool IsEditMode => Id > 0;
    public string PageTitle => IsEditMode ? "Edit Product" : "Create Product";
    public string SubmitButtonText => IsEditMode ? "Update Product" : "Create Product";
    public string SubmitButtonClass => IsEditMode ? "btn btn-primary" : "btn btn-success";
    
    // Breadcrumb navigation
    public List<BreadcrumbItem> Breadcrumbs => new()
    {
        new BreadcrumbItem("Home", "/"),
        new BreadcrumbItem("Products", "/Products"),
        new BreadcrumbItem(IsEditMode ? "Edit" : "Create", "")
    };
    
    // Validation summary customization
    public bool ShowValidationSummary { get; set; } = true;
    public string ValidationSummaryTitle { get; set; } = "Please correct the following errors:";
}

public class BreadcrumbItem
{
    public string Text { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    
    public BreadcrumbItem(string text, string url)
    {
        Text = text;
        Url = url;
    }
}
