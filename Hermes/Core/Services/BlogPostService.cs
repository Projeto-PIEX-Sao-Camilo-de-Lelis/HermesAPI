using Hermes.Core.Interfaces.Repository;
using Hermes.Core.Interfaces.Service;
using Hermes.Core.Models;
using Hermes.Helpers;
using Microsoft.Extensions.Hosting;

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

        public async Task<BlogPost> GetPostBySlugAsync(string slug)
        {
            return await _postRepository.GetBySlugAsync(slug);
        }

        public async Task<IEnumerable<BlogPost>> GetPostByAuthor(string author)
        {
            var posts = await _postRepository.GetByAuthorAsync(author);
            return posts.Where(post => post != null).Cast<BlogPost>();
        }

        public async Task<BlogPost> CreatePostAsync(BlogPost post, int contentPreviewMaxLength)
        {
            if (post is null)
            {
                throw new ArgumentNullException(nameof(post), "O post não pode ser nulo!");
            }

            bool slugExists = await CheckSlugExistsAsync(post);
            post.Slug = ShortHandSlugGenerator.GenerateUniqueSlug(post.Title, slugExists);
            post.ContentPreview = ContentPreviewGenerator.GeneratePreview(post.Content, contentPreviewMaxLength);
            post.CreatedAt = DateTime.UtcNow;

            return await _postRepository.CreateAsync(post);
        }

        public async Task<BlogPost> UpdatePostAsync(Guid id, BlogPost updatedPost)
        {
            var existingPost = await _postRepository.GetByIdAsync(id);

            if (existingPost is null)
            {
                throw new ArgumentNullException(nameof(existingPost), $"O post com id {id} não existe!");
            }

            if (updatedPost is null)
            {
                throw new ArgumentNullException(nameof(updatedPost), "O post não pode ser nulo!");
            }

            updatedPost.Slug = ShortHandSlugGenerator.GenerateSlug(updatedPost.Title);
            updatedPost.ContentPreview = ContentPreviewGenerator.GeneratePreview(updatedPost.Content);
            updatedPost.UpdatedAt = DateTime.UtcNow;

            return await _postRepository.UpdateAsync(id, updatedPost);
        }

        public async Task DeletePostAsync(Guid id)
        {
            await _postRepository.DeleteAsync(id);
        }

        private async Task<bool> CheckSlugExistsAsync(BlogPost post)
        {
            var generatedSlug = ShortHandSlugGenerator.GenerateSlug(post.Title);
            bool slugExists = await _postRepository.SlugExistsAsync(generatedSlug);
            return slugExists;
        }
    }
}