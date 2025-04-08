using Hermes.Core.Interfaces.Cache;
using Hermes.Core.Interfaces.Repository;
using Hermes.Core.Models;

namespace Hermes.Core.Services.Cache
{
    public class BlogPostCacheService : IBlogPostCacheService
    {
        private readonly ICacheProvider _cacheProvider;
        private readonly IBlogPostRepository _blogPostRepository;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(24);

        private const string PostByIdKeyPattern = "blogpost:id:{0}";
        private const string PostBySlugKeyPattern = "blogpost:slug:{0}";
        private const string AllPostsKey = "blogpost:all";
        private const string PagedPostsKeyPattern = "blogpost:paged:{0}:{1}";

        public BlogPostCacheService(ICacheProvider cacheProvider, IBlogPostRepository blogPostRepository)
        {
            _cacheProvider = cacheProvider;
            _blogPostRepository = blogPostRepository;
        }

        public async Task<BlogPost?> GetPostByIdAsync(Guid id)
        {
            string cacheKey = string.Format(PostByIdKeyPattern, id);

            var cachedPost = await _cacheProvider.GetAsync<BlogPost>(cacheKey);
            if (cachedPost is not null)
            {
                return cachedPost;
            }

            var post = await _blogPostRepository.GetByIdAsync(id);
            if (post is not null)
            {
                await _cacheProvider.SetAsync(cacheKey, post, _cacheExpiration);
            }

            return post;
        }

        public async Task<BlogPost?> GetPostBySlugAsync(string slug)
        {
            string cacheKey = string.Format(PostBySlugKeyPattern, slug);

            var cachedPost = await _cacheProvider.GetAsync<BlogPost>(cacheKey);
            if (cachedPost is not null)
            {
                return cachedPost;
            }

            var post = await _blogPostRepository.GetBySlugAsync(slug);
            if (post is not null)
            {
                await _cacheProvider.SetAsync(cacheKey, post, _cacheExpiration);
            }

            return post;
        }

        public async Task<IEnumerable<BlogPost>> GetAllPostsAsync()
        {
            var cachedPosts = await _cacheProvider.GetAsync<IEnumerable<BlogPost>>(AllPostsKey);
            if (cachedPosts is not null)
            {
                return cachedPosts;
            }

            var posts = await _blogPostRepository.GetAllAsync();
            if (posts is not null)
            {
                var nonNullPosts = posts.Where(post => post != null).Cast<BlogPost>().ToList();
                await _cacheProvider.SetAsync(AllPostsKey, nonNullPosts, _cacheExpiration);
                return nonNullPosts;
            }

            return Enumerable.Empty<BlogPost>();
        }

        public async Task<(IEnumerable<BlogPost> Posts, int TotalCount)> GetPagedPostsAsync(int pageNumber, int pageSize)
        {
            string cacheKey = string.Format(PagedPostsKeyPattern, pageNumber, pageSize);
           
            var cachedResult = await _cacheProvider.GetAsync<CachedPagedResult>(cacheKey);
            if (cachedResult is not null)
            {
                return (cachedResult.Posts, cachedResult.TotalCount);
            }

            var result = await _blogPostRepository.GetPagedAsync(pageNumber, pageSize);
            await _cacheProvider.SetAsync(cacheKey, new CachedPagedResult
            {
                Posts = result.Posts,
                TotalCount = result.TotalCount
            }, _cacheExpiration);

            return result;
        }

        public async Task CachePostAsync(BlogPost post)
        {
            await _cacheProvider.SetAsync(string.Format(PostByIdKeyPattern, post.Id), post, _cacheExpiration);
            await _cacheProvider.SetAsync(string.Format(PostBySlugKeyPattern, post.Slug), post, _cacheExpiration);

            await InvalidateAllPostsCacheAsync();
        }

        public async Task InvalidatePostCacheAsync(Guid id)
        {
            var post = await _blogPostRepository.GetByIdAsync(id);
            if (post is not null)
            {
                await _cacheProvider.RemoveAsync(string.Format(PostByIdKeyPattern, id));
                await _cacheProvider.RemoveAsync(string.Format(PostBySlugKeyPattern, post.Slug));
            }

            await InvalidateAllPostsCacheAsync();
        }

        public async Task InvalidateAllPostsCacheAsync()
        {
            await _cacheProvider.RemoveAsync(AllPostsKey);
            await _cacheProvider.ClearAsync("blogposts:paged:*");
        }

        private class CachedPagedResult
        {
            public IEnumerable<BlogPost> Posts { get; set; } = Enumerable.Empty<BlogPost>();
            public int TotalCount { get; set; }
        }
    }
}
