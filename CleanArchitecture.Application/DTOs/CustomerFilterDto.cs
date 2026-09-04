using CleanArchitecture.Application.Enums;

namespace CleanArchitecture.Application.DTOs;

/// <summary>
/// Query parameters for the paged customer list: paging, filtering and sorting.
/// </summary>
public class CustomerFilterDto : PagedFilterDto
{
    public string? SearchTerm { get; set; }

    public bool? IsActive { get; set; }

    public CustomerSortBy SortBy { get; set; } = CustomerSortBy.Name;

    public SortOrder SortOrder { get; set; } = SortOrder.Ascending;

    public bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(SearchTerm) ||
        IsActive.HasValue;
}
