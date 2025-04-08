using Hermes.Core.Models;

namespace Hermes.Core.Interfaces.Cache
{
    public interface IBlogPostCacheService
    {
        Task<BlogPost?> GetPostByIdAsync(Guid id);
        Task<BlogPost?> GetPostBySlugAsync(string slug);
        Task<IEnumerable<BlogPost>> GetAllPostsAsync();
        Task<(IEnumerable<BlogPost> Posts, int TotalCount)> GetPagedPostsAsync(int pageNumber, int pageSize);
        Task CachePostAsync(BlogPost post);
        Task AppendPostToCacheAsync(BlogPost newPost);
        Task InvalidatePostCacheAsync(Guid id);
        Task InvalidateAllPostsCacheAsync();
    }
}