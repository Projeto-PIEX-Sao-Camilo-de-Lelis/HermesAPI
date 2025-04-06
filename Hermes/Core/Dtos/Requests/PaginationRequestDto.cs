using Hermes.Configs.Constants;

namespace Hermes.Core.Dtos.Requests
{
    public record PaginationRequestDto
    {
        private int _pageNumber = PaginationConstants.DefaultPageNumber;
        private int _pageSize = PaginationConstants.DefaultPageSize;
        private const int MaxPageSize = PaginationConstants.DefaultMaxPageSize;

        public int PageNumber
        {
            get => _pageNumber;
            init => _pageNumber = value < 1 ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            init => _pageSize = value > MaxPageSize ? MaxPageSize : (value < 1 ? 10 : value);
        }
    }
}