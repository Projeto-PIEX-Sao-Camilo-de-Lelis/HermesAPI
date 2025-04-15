using Hermes.Core.Models;

namespace Hermes.Core.Interfaces.Service
{
    public interface IBlogPostService
    {
        Task<IEnumerable<BlogPost>> GetAllPostsAsync();
        Task<(IEnumerable<BlogPost> Posts, int TotalCount)> GetPagedPostsAsync(int pageNumber, int pageSize);
        Task<BlogPost> GetPostByIdAsync(Guid id);
        Task<BlogPost> GetPostBySlugAsync(string slug);
        Task<IEnumerable<BlogPost>> GetPostByAuthor(string author);
        Task<BlogPost> CreatePostAsync(BlogPost post, int contentPreviewMaxLength);
        Task<BlogPost> UpdatePostAsync(Guid id,BlogPost post);
        Task DeletePostAsync(Guid id);
        Task SynchronizeBlogPostsCacheAsync();
    }
}
