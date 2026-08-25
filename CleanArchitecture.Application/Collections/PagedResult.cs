using System.Collections;
using CleanArchitecture.Application.Interfaces.Collections;

namespace CleanArchitecture.Application.Collections;

public sealed class PagedResult<T> : IPaginatedList<T>
{
    private readonly IReadOnlyList<T> _items;

    public PagedResult(
        IReadOnlyList<T> items,
        int totalCount,
        int pageIndex,
        int pageSize)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageIndex, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        _items = items;
        TotalCount = totalCount;
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }

    public int PageIndex { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages { get; }

    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;

    public int Count => _items.Count;

    public T this[int index] => _items[index];

    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static PagedResult<T> Empty(
        int pageIndex = 1,
        int pageSize = 1)
        => new(Array.Empty<T>(), 0, pageIndex, pageSize);
}