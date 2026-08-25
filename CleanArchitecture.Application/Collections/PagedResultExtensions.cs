using CleanArchitecture.Application.Interfaces.Collections;

namespace CleanArchitecture.Application.Collections;

public static class PagedResultExtensions
{
    public static PagedResult<TDestination> ToPagedResult<TSource, TDestination>(
        this IPaginatedList<TSource> source,
        Func<TSource, TDestination> selector)
    {
       var items = source
            .Select(selector)
            .ToList();

        return new PagedResult<TDestination>(
            items,
            source.TotalCount,
            source.PageIndex,
            source.PageSize);
    }
}