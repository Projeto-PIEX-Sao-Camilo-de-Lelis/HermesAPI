using Hermes.Core.Interfaces.Repository;
using Hermes.Core.Interfaces.Service;
using Hermes.Core.Models;

namespace Hermes.Core.Services
{
    public class BlogPostService : IBlogPostService
    {
        private readonly IBlogPostRepository _postRepository;

        public BlogPostService(IBlogPostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        public async Task<IEnumerable<BlogPost>> GetAllPostsAsync()
        {
            return await _postRepository.GetAllAsync();
        }

        public async Task<(IEnumerable<BlogPost> Posts, int TotalCount)> GetPagedPostsAsync(int pageNumber, int pageSize)
        {
            return await _postRepository.GetPagedAsync(pageNumber, pageSize);
        }

        public async Task<BlogPost> GetPostByIdAsync(Guid id)
        {
            return await _postRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<BlogPost>> GetPostByAuthor(string author)
        {
            return await _postRepository.GetByAuthorAsync(author);
        }

        public async Task<BlogPost> CreatePostAsync(BlogPost post)
        {
            if (post is null)
            {
                throw new ArgumentNullException(nameof(post), "O post não pode ser nulo!");
            }

            return await _postRepository.CreateAsync(post);
        }

        public async Task<BlogPost> UpdatePostAsync(Guid id, BlogPost post)
        {
            if (post is null)
            {
                throw new ArgumentNullException(nameof(post), "O post não pode ser nulo!");
            }

            return await _postRepository.UpdateAsync(id, post);
        }

        public async Task DeletePostAsync(Guid id)
        {
            await _postRepository.DeleteAsync(id);
        }
    }
}