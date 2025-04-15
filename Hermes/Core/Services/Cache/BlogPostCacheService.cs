using Hermes.Configs.Cache;
using Hermes.Configs.Constants;
using Hermes.Core.Dtos.Responses;
using Hermes.Core.Interfaces.Cache;
using Hermes.Core.Interfaces.Repository;
using Hermes.Core.Models;

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
                var cachedResult = await _cacheProvider.GetAsync<BlogPostCachedPageResponseDto>(cacheKey);
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
                await _cacheProvider.SetAsync(cacheKey, new BlogPostCachedPageResponseDto
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
            try
            {
                await _cacheProvider.SetAsync(
                    string.Format(CacheConstants.PostByIdKeyPattern, post.Id),
                    post,
                    _cacheExpiration);

                await _cacheProvider.SetAsync(
                    string.Format(CacheConstants.PostBySlugKeyPattern, post.Slug),
                    post,
                    _cacheExpiration);

                var cachedPosts = await _cacheProvider.GetAsync<IEnumerable<BlogPost>>(CacheConstants.AllPostsKey);
                if (cachedPosts is not null)
                {
                    var postsList = cachedPosts.ToList();
                    var existingPostIndex = postsList.FindIndex(p => p.Id == post.Id);

                    if (existingPostIndex >= 0)
                    {
                        postsList[existingPostIndex] = post;
                    }
                    else
                    {
                        postsList.Add(post);
                    }

                    await _cacheProvider.SetAsync(CacheConstants.AllPostsKey, postsList, _cacheExpiration);
                }
                else
                {
                    var posts = await _blogPostRepository.GetAllAsync();
                    if (posts is not null)
                    {
                        var nonNullPosts = posts.Where(p => p != null).Cast<BlogPost>().ToList();
                        await _cacheProvider.SetAsync(CacheConstants.AllPostsKey, nonNullPosts, _cacheExpiration);
                    }
                    else
                    {
                        await _cacheProvider.SetAsync(CacheConstants.AllPostsKey, new List<BlogPost> { post }, _cacheExpiration);
                    }
                }

                await _cacheProvider.ClearAsync(CacheConstants.PagedPostsAsteriskKeyPattern);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao atualizar o cache para o post com ID: {Id}.", post.Id);
                await InvalidateAllPostsCacheAsync();
            }
        }

        public async Task AppendPostToCacheAsync(BlogPost newPost)
        {
            try
            {
                await _cacheProvider.SetAsync(
                    string.Format(CacheConstants.PostByIdKeyPattern, newPost.Id),
                    newPost,
                    _cacheExpiration);

                await _cacheProvider.SetAsync(
                    string.Format(CacheConstants.PostBySlugKeyPattern, newPost.Slug),
                    newPost,
                    _cacheExpiration);

                var allPostsFromDb = await _blogPostRepository.GetAllAsync();
                if (allPostsFromDb is not null)
                {
                    var nonNullPosts = allPostsFromDb.Where(post => post != null).Cast<BlogPost>().ToList();

                    if (!nonNullPosts.Any(p => p.Id == newPost.Id))
                    {
                        nonNullPosts.Add(newPost);
                    }

                    await _cacheProvider.SetAsync(CacheConstants.AllPostsKey, nonNullPosts, _cacheExpiration);
                }
                else
                {
                    await _cacheProvider.SetAsync(CacheConstants.AllPostsKey, new List<BlogPost> { newPost }, _cacheExpiration);
                }

                await _cacheProvider.ClearAsync(CacheConstants.PagedPostsAsteriskKeyPattern);

                _logger.LogInformation("Cache de post atualizado com sucesso para o post ID: {Id}, título: {Title}",
                    newPost.Id, newPost.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao tentar adicionar o post ao cache. ID: {Id}, título: {Title}",
                    newPost.Id, newPost.Title);

                await InvalidateAllPostsCacheAsync();
            }
        }

        public async Task UpdatePostInCacheAsync(BlogPost updatedPost)
        {
            try
            {
                await _cacheProvider.SetAsync(
                    string.Format(CacheConstants.PostByIdKeyPattern, updatedPost.Id),
                    updatedPost,
                    _cacheExpiration);

                await _cacheProvider.SetAsync(
                    string.Format(CacheConstants.PostBySlugKeyPattern, updatedPost.Slug),
                    updatedPost,
                    _cacheExpiration);

                var cachedPosts = await _cacheProvider.GetAsync<IEnumerable<BlogPost>>(CacheConstants.AllPostsKey);
                if (cachedPosts is not null)
                {
                    var postsList = cachedPosts.ToList();
                    var existingPostIndex = postsList.FindIndex(p => p.Id == updatedPost.Id);

                    if (existingPostIndex >= 0)
                    {
                        postsList[existingPostIndex] = updatedPost;
                        await _cacheProvider.SetAsync(CacheConstants.AllPostsKey, postsList, _cacheExpiration);
                    }
                    else
                    {
                        var allPosts = await _blogPostRepository.GetAllAsync();
                        if (allPosts is not null)
                        {
                            var nonNullPosts = allPosts.Where(post => post != null).Cast<BlogPost>().ToList();
                            await _cacheProvider.SetAsync(CacheConstants.AllPostsKey, nonNullPosts, _cacheExpiration);
                        }
                    }
                }
                else
                {
                    var posts = await _blogPostRepository.GetAllAsync();
                    if (posts is not null)
                    {
                        var nonNullPosts = posts.Where(post => post != null).Cast<BlogPost>().ToList();
                        await _cacheProvider.SetAsync(CacheConstants.AllPostsKey, nonNullPosts, _cacheExpiration);
                    }
                }

                await _cacheProvider.ClearAsync(CacheConstants.PagedPostsAsteriskKeyPattern);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao tentar atualizar o post com o ID: {Id} no cache.", updatedPost.Id);
                await InvalidateAllPostsCacheAsync();
            }
        }

        public async Task DeletePostFromCacheAsync(Guid id, string? slug = null)
        {
            try
            {
                await _cacheProvider.RemoveAsync(string.Format(CacheConstants.PostByIdKeyPattern, id));

                if (!string.IsNullOrEmpty(slug))
                {
                    await _cacheProvider.RemoveAsync(string.Format(CacheConstants.PostBySlugKeyPattern, slug));
                }
                else
                {
                    var cachedPosts = await _cacheProvider.GetAsync<IEnumerable<BlogPost>>(CacheConstants.AllPostsKey);
                    if (cachedPosts is not null)
                    {
                        var post = cachedPosts.FirstOrDefault(p => p.Id == id);
                        if (post?.Slug is not null)
                        {
                            await _cacheProvider.RemoveAsync(string.Format(CacheConstants.PostBySlugKeyPattern, post.Slug));
                        }
                    }
                }

                var allCachedPosts = await _cacheProvider.GetAsync<IEnumerable<BlogPost>>(CacheConstants.AllPostsKey);
                if (allCachedPosts is not null)
                {
                    var updatedPostsList = allCachedPosts.Where(p => p.Id != id).ToList();

                    await _cacheProvider.SetAsync(CacheConstants.AllPostsKey, updatedPostsList, _cacheExpiration);
                }
                else
                {
                    var posts = await _blogPostRepository.GetAllAsync();
                    if (posts is not null)
                    {
                        var nonNullPosts = posts.Where(post => post != null && post.Id != id).Cast<BlogPost>().ToList();
                        await _cacheProvider.SetAsync(CacheConstants.AllPostsKey, nonNullPosts, _cacheExpiration);
                    }
                }

                await _cacheProvider.ClearAsync(CacheConstants.PagedPostsAsteriskKeyPattern);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao tentar excluir o post com o ID: {Id} do cache.", id);
                await InvalidateAllPostsCacheAsync();
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
            await _cacheProvider.ClearAsync(CacheConstants.PagedPostsAsteriskKeyPattern);
        }
    }
}
