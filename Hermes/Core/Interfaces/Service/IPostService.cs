using Hermes.Core.Models;

namespace Hermes.Core.Interfaces.Service
{
    public interface IPostService
    {
        Task<IEnumerable<Post>> GetAllPostsAsync();
        Task<Post> GetPostByIdAsync(Guid id);
        Task<IEnumerable<Post>> GetPostByAuthor(string author);
        Task<Post> CreatePostAsync(Post post);
        Task<Post> UpdatePostAsync(Guid id,Post post);
        Task DeletePostAsync(Guid id);
    }
}
