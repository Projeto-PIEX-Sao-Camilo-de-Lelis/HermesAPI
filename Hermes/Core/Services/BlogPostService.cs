using Hermes.Core.Interfaces.Cache;
using Hermes.Core.Interfaces.Repository;
using Hermes.Core.Interfaces.Service;
using Hermes.Core.Models;
using Hermes.Helpers;

namespace Hermes.Core.Services
{
    public class BlogPostService : IBlogPostService
    {
        private readonly IBlogPostRepository _postRepository;
        private readonly IBlogPostCacheService _cacheService;

        public BlogPostService(IBlogPostRepository postRepository, IBlogPostCacheService cacheService)
        {
            _postRepository = postRepository;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<BlogPost>> GetAllPostsAsync()
        {
            return await _cacheService.GetAllPostsAsync();
        }

        public async Task<(IEnumerable<BlogPost> Posts, int TotalCount)> GetPagedPostsAsync(int pageNumber, int pageSize)
        {
            return await _cacheService.GetPagedPostsAsync(pageNumber, pageSize);
        }

        public async Task<BlogPost> GetPostByIdAsync(Guid id)
        {
            return await _cacheService.GetPostByIdAsync(id);
        }

        public async Task<BlogPost> GetPostBySlugAsync(string slug)
        {
            return await _cacheService.GetPostBySlugAsync(slug);
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

            var createdPost = await _postRepository.CreateAsync(post);
            if (createdPost is null)
            {
                throw new InvalidOperationException("Ocorreu um erro ao tentar criar o post.");
            }

            if (createdPost is not null)
            {
                await _cacheService.AppendPostToCacheAsync(createdPost);
            }

            return createdPost;
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

            var result = await _postRepository.UpdateAsync(id, updatedPost);
            if (result is null)
            {
                throw new InvalidOperationException("Ocorreu um erro ao tentar atualizar o post.");
            }

            await _cacheService.InvalidatePostCacheAsync(id);

            return result;
        }

        public async Task DeletePostAsync(Guid id)
        {
            await _postRepository.DeleteAsync(id);
            await _cacheService.InvalidatePostCacheAsync(id);
        }

        public async Task SynchronizeBlogPostsCacheAsync()
        {
            await _cacheService.InvalidateAllPostsCacheAsync();
            var allPosts = await _postRepository.GetAllAsync();

            foreach (var post in allPosts)
            {
                await _cacheService.CachePostAsync(post);
            }
        }

        private async Task<bool> CheckSlugExistsAsync(BlogPost post)
        {
            var generatedSlug = ShortHandSlugGenerator.GenerateSlug(post.Title);
            bool slugExists = await _postRepository.SlugExistsAsync(generatedSlug);
            return slugExists;
        }
    }
}