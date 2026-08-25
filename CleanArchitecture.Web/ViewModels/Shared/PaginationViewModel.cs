using CleanArchitecture.Application.Collections;
using CleanArchitecture.Application.DTOs;

namespace CleanArchitecture.ViewModels.Shared
{
    public class PaginationViewModel(PagedResult<ProductDto> list)
    {
        public int CurrentPage { get; set; } = list.PageIndex;
        public int TotalItems { get; set; } = list.TotalCount;
        public int PageSize { get; set; } = list.PageSize;
        public int PaginationId { get; set; }
        public string ContainerCssClasses { get; set; } = "d-flex mt-3";
        public bool ShowPageInfo { get; set; } = true;

        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public int StartItem => (CurrentPage - 1) * PageSize + 1;
        public int EndItem => Math.Min(CurrentPage * PageSize, TotalItems);
        public bool HasMultiplePages => TotalPages > 1;

        public int StartPage => Math.Max(1, CurrentPage - 2);
        public int EndPage => Math.Min(TotalPages, CurrentPage + 2);
    }
}
