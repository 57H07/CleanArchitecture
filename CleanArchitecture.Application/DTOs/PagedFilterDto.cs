namespace CleanArchitecture.Application.DTOs;

/// <summary>
/// Shared paging contract for paged/filtered list query DTOs: clamps page to >= 1
/// and page size to the 1..100 range.
/// </summary>
public abstract class PagedFilterDto
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
}
