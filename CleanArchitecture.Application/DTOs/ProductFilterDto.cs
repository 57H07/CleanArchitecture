using CleanArchitecture.Application.Enums;
using CleanArchitecture.Domain.Enums;

namespace CleanArchitecture.Application.DTOs;

/// <summary>
/// Query parameters for the paged product list: paging, filtering and sorting.
/// </summary>
public class ProductFilterDto
{
    private const int MaxPageSize = 100;

    private int _page = 1;
    private int _pageSize = 5;

    /// <summary>1-based page index. Values below 1 are clamped to 1.</summary>
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    /// <summary>Page size, clamped to the 1..100 range.</summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 1 : Math.Min(value, MaxPageSize);
    }

    public string? SearchTerm { get; set; }

    public string? Category { get; set; }

    public ProductStatus? Status { get; set; }

    public int? UserId { get; set; }

    public ProductSortBy SortBy { get; set; } = ProductSortBy.Name;

    public SortOrder SortOrder { get; set; } = SortOrder.Ascending;

    public bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(SearchTerm) ||
        !string.IsNullOrWhiteSpace(Category) ||
        Status.HasValue ||
        UserId.HasValue;
}
