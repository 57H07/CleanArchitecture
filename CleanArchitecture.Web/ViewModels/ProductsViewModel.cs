using CleanArchitecture.Application.Collections;
using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Application.Enums;
using CleanArchitecture.Domain.Enums;
using CleanArchitecture.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CleanArchitecture.Web.ViewModels;

public class ProductsViewModel
{
    public required PaginatedList<ProductDto> Products { get; init; }
    public required PaginationViewModel Pagination { get; init; }

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
