namespace Hermes.Core.Dtos.Responses
{
    public record PagedResponseDto<T>
    {
        public IEnumerable<T> Posts { get; }
        public int PageNumber { get; }
        public int PageSize { get; }
        public int TotalCount { get; }
        public int TotalPages { get; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public PagedResponseDto(IEnumerable<T> posts, int pageNumber, int pageSize, int totalCount)
        {
            Posts = posts;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalCount = totalCount;
            TotalPages = (int)Math.Ceiling(totalCount / (double)(pageSize));
        }
    }
}