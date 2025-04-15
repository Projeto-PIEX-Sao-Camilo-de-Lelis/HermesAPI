using Hermes.Core.Models;

namespace Hermes.Core.Dtos.Responses
{
    public record BlogPostCachedPageResponseDto
    {
        public required IEnumerable<BlogPost> Posts { get; set; } = Enumerable.Empty<BlogPost>();
        public required int TotalCount { get; set; }
    }
}