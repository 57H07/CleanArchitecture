using CleanArchitecture.Application.Interfaces.Collections;

namespace CleanArchitecture.Application.Collections;

public class PaginatedList<T> : List<T>, IPaginatedList
{
    private const int DefaultPageSize = 10;
    private int _pageIndex;
    private int _totalPages;
    private int _totalCount;
    private int _pageSize;

    public int PageIndex
    {
        get => _pageIndex;
        private set => _pageIndex = NormalizePageIndex(value, TotalPages);
    }

    public int TotalPages
    {
        get => _totalPages;
        private set => _totalPages = Math.Max(0, value);
    }

    public int TotalCount
    {
        get => _totalCount;
        private set => _totalCount = Math.Max(0, value);
    }

    public int PageSize
    {
        get => _pageSize;
        private set => _pageSize = NormalizePageSize(value);
    }
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;

    public PaginatedList(IEnumerable<T> items, int pageIndex, int totalPages, int totalCount, int pageSize)
    {
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = totalPages > 0 ? totalPages : ComputeTotalPages(TotalCount, PageSize);
        PageIndex = pageIndex;
        AddRange(items);
    }

    public PaginatedList(IEnumerable<T> items, int count, int pageIndex, int pageSize)
    {
        PageSize = pageSize;
        TotalCount = count;
        TotalPages = ComputeTotalPages(TotalCount, PageSize);
        PageIndex = pageIndex;
        AddRange(items);
    }

    public static Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
    {
        return Task.FromResult(Create(source, pageIndex, pageSize));
    }

    public static PaginatedList<T> Create(IQueryable<T> source, int pageIndex, int pageSize)
    {
        var normalizedPageSize = NormalizePageSize(pageSize);
        var count = source.Count();
        var totalPages = ComputeTotalPages(count, normalizedPageSize);
        var normalizedPageIndex = NormalizePageIndex(pageIndex, totalPages);
        var items = source.Skip((normalizedPageIndex - 1) * normalizedPageSize).Take(normalizedPageSize).ToList();
        return new PaginatedList<T>(items, normalizedPageIndex, totalPages, count, normalizedPageSize);
    }

    public int GetStartIndex()
    {
        return (PageIndex - 1) * PageSize + 1;
    }

    public int GetEndIndex()
    {
        return Math.Min(PageIndex * PageSize, TotalCount);
    }

    public bool IsValidPage()
    {
        return PageIndex >= 1 && PageIndex <= TotalPages;
    }

    private static int NormalizePageSize(int pageSize)
    {
        return pageSize < 1 ? DefaultPageSize : pageSize;
    }

    private static int ComputeTotalPages(int totalCount, int pageSize)
    {
        return pageSize > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0;
    }

    private static int NormalizePageIndex(int pageIndex, int totalPages)
    {
        if (totalPages <= 0)
        {
            return 1;
        }

        var normalizedIndex = pageIndex < 1 ? 1 : pageIndex;
        return Math.Min(normalizedIndex, totalPages);
    }
}
