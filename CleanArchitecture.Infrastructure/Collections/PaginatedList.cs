using Microsoft.EntityFrameworkCore;
using CleanArchitecture.Application.Interfaces.Collections;

namespace CleanArchitecture.Infrastructure.Collections
{
    public class PaginatedList<T> : IPaginatedList<T>
    {
        private readonly IReadOnlyList<T> _items;

        private PaginatedList(IReadOnlyList<T> items, int totalCount, int pageIndex, int pageSize)
        {
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
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        public static async Task<PaginatedList<T>> CreateAsync(
            IQueryable<T> source,
            int pageIndex,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(pageIndex, 1);
            ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

            int count = await source.CountAsync(cancellationToken);
            List<T> items = await source
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }

        public static PaginatedList<T> FromMaterialized(IReadOnlyList<T> items, int totalCount, int pageIndex, int pageSize)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(pageIndex, 1);
            ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

            return new PaginatedList<T>(items, totalCount, pageIndex, pageSize);
        }
    }
}
