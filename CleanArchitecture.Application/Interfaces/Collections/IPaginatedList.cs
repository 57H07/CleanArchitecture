namespace CleanArchitecture.Application.Interfaces.Collections;

public interface IPaginatedList<out T> : IReadOnlyList<T>
{
    int PageIndex { get; }

    int PageSize { get; }

    int TotalCount { get; }

    int TotalPages { get; }

    bool HasPreviousPage { get; }
    bool HasNextPage { get; }
}
