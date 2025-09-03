using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Application.Enums;
using CleanArchitecture.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CleanArchitecture.Web.ViewModels;

/// <summary>
/// ViewModel for the Products Index/List page
/// Combines DTOs with UI-specific functionality
/// </summary>
public class ProductListViewModel
{
    // Data from application layer
    public IEnumerable<ProductDto> Products { get; set; } = Enumerable.Empty<ProductDto>();
    
    // Pagination properties
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    
    // Search and filtering
    public string SearchTerm { get; set; } = string.Empty;
    public string SelectedCategory { get; set; } = string.Empty;
    public ProductStatus? SelectedStatus { get; set; }
    public int? SelectedUserId { get; set; }
    
    // Sorting
    public ProductSortBy SortBy { get; set; } = ProductSortBy.Name;
    public SortOrder SortOrder { get; set; } = SortOrder.Ascending;
    
    // UI dropdowns
    public SelectList? AvailableCategories { get; set; }
    public SelectList? AvailableStatuses { get; set; }
    public SelectList? AvailableUsers { get; set; }
    public SelectList? SortOptions { get; set; }
    
    // Display properties
    public string PageTitle { get; set; } = "Products";
    public string SearchPlaceholder { get; set; } = "Search products...";
    
    // Action buttons configuration
    public bool ShowCreateButton { get; set; } = true;
    public bool ShowExportButton { get; set; } = true;
    public bool ShowBulkActions { get; set; } = false;
    
    // View state
    public bool HasResults => Products.Any();
    public string NoResultsMessage => !string.IsNullOrEmpty(SearchTerm) 
        ? $"No products found for '{SearchTerm}'" 
        : "No products available";
    
    // Summary information
    public int TotalActiveProducts => Products.Count(p => p.Status == ProductStatus.Active);
    public int TotalDraftProducts => Products.Count(p => p.Status == ProductStatus.Draft);
    public decimal TotalInventoryValue => Products.Sum(p => p.Price * p.StockQuantity);
    
    // Helper methods for view
    public string GetSortIcon(ProductSortBy sortField)
    {
        if (SortBy != sortField) return "bi-arrow-up-down";
        return SortOrder == SortOrder.Ascending ? "bi-arrow-up" : "bi-arrow-down";
    }
    
    public string GetSortUrl(ProductSortBy sortField)
    {
        var newOrder = SortBy == sortField && SortOrder == SortOrder.Ascending 
            ? SortOrder.Descending 
            : SortOrder.Ascending;
        
        return $"?sortBy={sortField}&sortOrder={newOrder}&searchTerm={SearchTerm}&selectedCategory={SelectedCategory}";
    }
    
    public string GetPaginationUrl(int page)
    {
        return $"?page={page}&sortBy={SortBy}&sortOrder={SortOrder}&searchTerm={SearchTerm}&selectedCategory={SelectedCategory}";
    }
}
