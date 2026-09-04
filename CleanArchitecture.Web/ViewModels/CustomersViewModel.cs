using CleanArchitecture.Application.Collections;
using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Application.Enums;

namespace CleanArchitecture.Web.ViewModels;

public class CustomersViewModel
{
    public required PagedResult<CustomerDto> Customers { get; init; }

    public required CustomerFilterDto Filter { get; init; }

    // Pagination
    public int CurrentPage => Customers.PageIndex;
    public int PageSize => Customers.PageSize;
    public int TotalItems => Customers.TotalCount;
    public int TotalPages => Customers.TotalPages;
    public bool HasPreviousPage => Customers.HasPreviousPage;
    public bool HasNextPage => Customers.HasNextPage;
    public bool HasMultiplePages => TotalPages > 1;
    public int StartItem => TotalItems == 0 ? 0 : (CurrentPage - 1) * PageSize + 1;
    public int EndItem => Math.Min(CurrentPage * PageSize, TotalItems);

    // View state
    public bool HasResults => Customers.Any();
    public string NoResultsMessage => Filter.HasActiveFilters
        ? "No customers match the current filters"
        : "No customers available";

    public string GetSortIcon(CustomerSortBy sortField)
    {
        if (Filter.SortBy != sortField) return "bi-arrow-down-up text-muted";
        return Filter.SortOrder == SortOrder.Ascending ? "bi-arrow-up" : "bi-arrow-down";
    }

    public IDictionary<string, string> GetSortRouteValues(CustomerSortBy sortField)
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

    private IDictionary<string, string> BuildRouteValues(int page, CustomerSortBy sortBy, SortOrder sortOrder)
    {
        var values = new Dictionary<string, string>
        {
            ["page"] = page.ToString(),
            ["pageSize"] = PageSize.ToString(),
            ["sortBy"] = sortBy.ToString(),
            ["sortOrder"] = sortOrder.ToString()
        };

        if (!string.IsNullOrWhiteSpace(Filter.SearchTerm)) values["searchTerm"] = Filter.SearchTerm;
        if (Filter.IsActive.HasValue) values["isActive"] = Filter.IsActive.Value.ToString();

        return values;
    }
}
