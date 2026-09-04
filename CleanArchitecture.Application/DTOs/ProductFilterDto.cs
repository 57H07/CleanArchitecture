using CleanArchitecture.Application.Enums;
using CleanArchitecture.Domain.Enums;

namespace CleanArchitecture.Application.DTOs;

/// <summary>
/// Query parameters for the paged product list: paging, filtering and sorting.
/// </summary>
public class ProductFilterDto : PagedFilterDto
{
    public string? SearchTerm { get; set; }

    public string? Category { get; set; }

    public ProductStatus? Status { get; set; }

    public int? CustomerId { get; set; }

    public ProductSortBy SortBy { get; set; } = ProductSortBy.Name;

    public SortOrder SortOrder { get; set; } = SortOrder.Ascending;

    public bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(SearchTerm) ||
        !string.IsNullOrWhiteSpace(Category) ||
        Status.HasValue ||
        CustomerId.HasValue;
}
