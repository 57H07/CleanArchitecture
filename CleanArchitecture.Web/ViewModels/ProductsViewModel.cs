using CleanArchitecture.Application.Collections;
using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Application.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CleanArchitecture.Web.ViewModels;

public class ProductsViewModel
{
    public required PagedResult<ProductDto> Products { get; init; }

    public required ProductFilterDto Filter { get; init; }

    // UI dropdowns
    public SelectList? AvailableCategories { get; set; }
    public SelectList? AvailableStatuses { get; set; }
    public SelectList? AvailableCustomers { get; set; }

    // Pagination
    public int CurrentPage => Products.PageIndex;
    public int PageSize => Products.PageSize;
    public int TotalItems => Products.TotalCount;
    public int TotalPages => Products.TotalPages;
    public bool HasPreviousPage => Products.HasPreviousPage;
    public bool HasNextPage => Products.HasNextPage;
    public bool HasMultiplePages => TotalPages > 1;
    public int StartItem => TotalItems == 0 ? 0 : (CurrentPage - 1) * PageSize + 1;
    public int EndItem => Math.Min(CurrentPage * PageSize, TotalItems);

    // View state
    public bool HasResults => Products.Any();
    public string NoResultsMessage => Filter.HasActiveFilters
        ? "No products match the current filters"
        : "No products available";

    // Helper methods for view: build query strings that preserve current filter/sort/page state
    public string GetSortIcon(ProductSortBy sortField)
    {
        if (Filter.SortBy != sortField) return "bi-arrow-down-up text-muted";
        return Filter.SortOrder == SortOrder.Ascending ? "bi-arrow-up" : "bi-arrow-down";
    }

    public IDictionary<string, string> GetSortRouteValues(ProductSortBy sortField)
    {
        var newOrder = Filter.SortBy == sortField && Filter.SortOrder == SortOrder.Ascending
            ? SortOrder.Descending
            : SortOrder.Ascending;

        return BuildRouteValues(page: 1, sortBy: sortField, sortOrder: newOrder);
    }

    public IDictionary<string, string> GetPageRouteValues(int page)
    {
        return BuildRouteValues(page, Filter.SortBy, Filter.SortOrder);
    }

    private IDictionary<string, string> BuildRouteValues(int page, ProductSortBy sortBy, SortOrder sortOrder)
    {
        var values = new Dictionary<string, string>
        {
            ["page"] = page.ToString(),
            ["pageSize"] = PageSize.ToString(),
            ["sortBy"] = sortBy.ToString(),
            ["sortOrder"] = sortOrder.ToString()
        };

        if (!string.IsNullOrWhiteSpace(Filter.SearchTerm)) values["searchTerm"] = Filter.SearchTerm;
        if (!string.IsNullOrWhiteSpace(Filter.Category)) values["category"] = Filter.Category;
        if (Filter.Status.HasValue) values["status"] = Filter.Status.Value.ToString();
        if (Filter.CustomerId.HasValue) values["customerId"] = Filter.CustomerId.Value.ToString();

        return values;
    }
}
