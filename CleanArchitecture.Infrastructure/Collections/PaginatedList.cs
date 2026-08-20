using Microsoft.EntityFrameworkCore;
using CleanArchitecture.Application.Interfaces.Collections;

namespace CleanArchitecture.Infrastructure.Collections
{
    public class PaginatedList<T> : List<T>, IPaginatedList
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }
        public int TotalCount { get; private set; }
        public int PageSize { get; private set; }
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public PaginatedList(IEnumerable<T> items, int pageIndex, int totalPages, int totalCount,  int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = totalPages;
            TotalCount = totalCount;
            PageSize = pageSize;
            AddRange(items);
        }

        public PaginatedList(IEnumerable<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            TotalCount = count;
            PageSize = pageSize;
            AddRange(items);
        }

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }

        public static PaginatedList<T> Create(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = source.Count();
            var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
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
    }
}
