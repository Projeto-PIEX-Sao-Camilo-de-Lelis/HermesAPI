using Hermes.Configs.Cache;
using Hermes.Configs.Constants;
using Hermes.Core.Interfaces.Cache;
using Hermes.Core.Interfaces.Repository;
using Hermes.Core.Models;
using Microsoft.Extensions.Hosting;

namespace Hermes.Core.Services.Cache
{
    public class BlogPostCacheService : IBlogPostCacheService
    {
        private readonly ICacheProvider _cacheProvider;
        private readonly IBlogPostRepository _blogPostRepository;
        private readonly TimeSpan _cacheExpiration;
        private readonly ILogger<BlogPostCacheService> _logger;

        public BlogPostCacheService(
            ICacheProvider cacheProvider, 
            IBlogPostRepository blogPostRepository, 
            CacheSettings cacheSettings, 
            ILogger<BlogPostCacheService> logger)
        {
            _cacheProvider = cacheProvider;
            _blogPostRepository = blogPostRepository;
            _cacheExpiration = TimeSpan.FromMinutes(cacheSettings.Expiration);
            _logger = logger;
        }

        public async Task<BlogPost?> GetPostByIdAsync(Guid id)
        {
            string cacheKey = string.Format(CacheConstants.PostByIdKeyPattern, id);

            try
            {
                var cachedPost = await _cacheProvider.GetAsync<BlogPost>(cacheKey);
                if (cachedPost is not null)
                {
                    return cachedPost;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao tentar obter o post com o ID: {Id} do cache", id);
            }

            var post = await _blogPostRepository.GetByIdAsync(id);
            if (post is not null)
            {
                try
                {
                    await _cacheProvider.SetAsync(cacheKey, post, _cacheExpiration);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ocorreu um erro ao tentar inserir o post com o ID: {Id} no cache.", id);
                }
            }

            return post;
        }

        public async Task<BlogPost?> GetPostBySlugAsync(string slug)
        {
            string cacheKey = string.Format(CacheConstants.PostBySlugKeyPattern, slug);

            try
            {
                var cachedPost = await _cacheProvider.GetAsync<BlogPost>(cacheKey);
                if (cachedPost is not null)
                {
                    return cachedPost;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao tentar obter o post a slug: {Slug} do cache.", slug);
            }

            var post = await _blogPostRepository.GetBySlugAsync(slug);
            if (post is not null)
            {
                try
                {
                    await _cacheProvider.SetAsync(cacheKey, post, _cacheExpiration);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ocorreu um erro ao tentar inserir o post com a slug: {Slug} no cache.", slug);
                }
            }

            return post;
        }

        public async Task<IEnumerable<BlogPost>> GetAllPostsAsync()
        {
            try
            {
                var cachedPosts = await _cacheProvider.GetAsync<IEnumerable<BlogPost>>(CacheConstants.AllPostsKey);
                if (cachedPosts is not null)
                {
                    return cachedPosts;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao tentar obter os posts do cache.");
            }

            var posts = await _blogPostRepository.GetAllAsync();
            if (posts is not null)
            {
                var nonNullPosts = posts.Where(post => post != null).Cast<BlogPost>().ToList();
                try
                {
                    await _cacheProvider.SetAsync(CacheConstants.AllPostsKey, nonNullPosts, _cacheExpiration);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ocorreu um erro ao tentar inserir os posts no cache.");
                }

                return nonNullPosts;
            }

            return Enumerable.Empty<BlogPost>();
        }

        public async Task<(IEnumerable<BlogPost> Posts, int TotalCount)> GetPagedPostsAsync(int pageNumber, int pageSize)
        {
            string cacheKey = string.Format(CacheConstants.PagedPostsKeyPattern, pageNumber, pageSize);

            try
            {
                var cachedResult = await _cacheProvider.GetAsync<CachedPagedResult>(cacheKey);
                if (cachedResult is not null)
                {
                    return (cachedResult.Posts, cachedResult.TotalCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao tentar obter os posts paginados do cache.");
            }

            var result = await _blogPostRepository.GetPagedAsync(pageNumber, pageSize);
            try
            {
                await _cacheProvider.SetAsync(cacheKey, new CachedPagedResult
                {
                    Posts = result.Posts,
                    TotalCount = result.TotalCount
                }, _cacheExpiration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao tentar inserir os posts paginados no cache.");
            }

            return result;
        }

        public async Task CachePostAsync(BlogPost post)
        {
            await _cacheProvider.SetAsync(string.Format(CacheConstants.PostByIdKeyPattern, post.Id), post, _cacheExpiration);
            await _cacheProvider.SetAsync(string.Format(CacheConstants.PostBySlugKeyPattern, post.Slug), post, _cacheExpiration);

            await InvalidateAllPostsCacheAsync();
        }

        public async Task AppendPostToCacheAsync(BlogPost newPost)
        {
            try
            {
                var cachedPosts = await _cacheProvider.GetAsync<IEnumerable<BlogPost>>(CacheConstants.AllPostsKey);
                if (cachedPosts is not null)
                {
                    var refreshedPostsCache = cachedPosts.Append(newPost).ToList();
                    await _cacheProvider.SetAsync(CacheConstants.AllPostsKey, refreshedPostsCache, _cacheExpiration);
                }
                else
                {
                    await _cacheProvider.SetAsync(CacheConstants.AllPostsKey, new List<BlogPost> { newPost }, _cacheExpiration);
                }

                await _cacheProvider.SetAsync(string.Format(CacheConstants.PostByIdKeyPattern, newPost.Id), newPost, _cacheExpiration);
                await _cacheProvider.SetAsync(string.Format(CacheConstants.PostBySlugKeyPattern, newPost.Slug), newPost, _cacheExpiration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao tentar adicionar o post ao cache.");
            }
        }

        public async Task InvalidatePostCacheAsync(Guid id)
        {
            var post = await _blogPostRepository.GetByIdAsync(id);
            if (post is not null)
            {
                await _cacheProvider.RemoveAsync(string.Format(CacheConstants.PostByIdKeyPattern, id));
                await _cacheProvider.RemoveAsync(string.Format(CacheConstants.PostBySlugKeyPattern, post.Slug));
            }

            await InvalidateAllPostsCacheAsync();
        }

        public async Task InvalidateAllPostsCacheAsync()
        {
            await _cacheProvider.RemoveAsync(CacheConstants.AllPostsKey);
            await _cacheProvider.ClearAsync("blogposts:paged:*");
        }

        private class CachedPagedResult
        {
            public IEnumerable<BlogPost> Posts { get; set; } = Enumerable.Empty<BlogPost>();
            public int TotalCount { get; set; }
        }
    }
}
