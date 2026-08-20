namespace CleanArchitecture.Application.Interfaces.Collections
{
    public interface IPaginatedList
    {
        bool HasNextPage { get; }
        bool HasPreviousPage { get; }
        int PageIndex { get; }
        int PageSize { get; }
        int TotalCount { get; }
        int TotalPages { get; }

        int GetEndIndex();
        int GetStartIndex();
        bool IsValidPage();
    }
}
