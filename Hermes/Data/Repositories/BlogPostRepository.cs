using Dapper;
using Hermes.Core.Interfaces.Data;
using Hermes.Core.Interfaces.Repository;
using Hermes.Core.Models;
using Hermes.Helpers;

namespace Hermes.Data.Repositories
{
    public class BlogPostRepository : BaseRepository, IBlogPostRepository
    {
        public BlogPostRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory) { }

        public async Task<IEnumerable<BlogPost?>> GetAllAsync()
        {
            return await ExecuteWithConnectionAsync(async connection =>
            {
                const string sql = @"
                    SELECT 
                        p.id,
                        p.slug,
                        p.title,
                        p.content,
                        p.content_preview,
                        p.author_id,
                        p.published_at,
                        p.created_at,
                        p.updated_at,
                        p.is_published,
                        u.name AS author 
                    FROM posts AS p 
                    JOIN users AS u 
                        ON p.author_id = u.id 
                    WHERE is_published = true 
                    ORDER BY p.created_at DESC";

                return await connection.QueryAsync<BlogPost>(sql);
            });
        }

        public async Task<(IEnumerable<BlogPost> Posts, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
        {
            var offset = (pageNumber - 1) * pageSize;

            const string postsSql = @"
                SELECT 
                      p.id,
                      p.slug,
                      p.title,
                      p.content,
                      p.content_preview,
                      p.author_id,
                      p.published_at, 
                      p.created_at,
                      p.updated_at,
                      p.is_published,
                      u.id AS author_id,
                      u.name AS author
                FROM posts AS p
                JOIN users AS u
                    ON p.author_id = u.id 
                WHERE is_published = true
                ORDER BY p.created_at DESC
                LIMIT @PageSize OFFSET @Offset";

            const string countSql = @"
                SELECT COUNT(*)
                FROM posts
                WHERE is_published = true";

            IEnumerable<BlogPost> posts = Enumerable.Empty<BlogPost>();
            int totalCount = 0;

            await ExecuteWithConnectionAsync(async connection =>
            {
                posts = await connection.QueryAsync<BlogPost>(postsSql, new { PageSize = pageSize, Offset = offset });
            });

            await ExecuteWithConnectionAsync(async connection =>
            {
                totalCount = await connection.ExecuteScalarAsync<int>(countSql);
            });

            return (posts, totalCount);
        }

        public async Task<BlogPost?> GetByIdAsync(Guid id)
        {
            return await ExecuteWithConnectionAsync(async connection =>
            {
                const string sql = @"
                    SELECT 
                        p.id,
                        p.slug,
                        p.title,
                        p.content,
                        p.content_preview,
                        p.author_id,
                        p.published_at,
                        p.created_at,
                        p.updated_at,
                        p.is_published,
                        u.name AS author 
                    FROM posts AS p 
                    JOIN users AS u 
                        ON p.author_id = u.id 
                    WHERE p.id = @Id AND is_published = true 
                    ORDER BY p.created_at DESC";

                return await connection.QueryFirstOrDefaultAsync<BlogPost>(sql, new { Id = id });
            });
        }

        public async Task<BlogPost?> GetBySlugAsync(string slug)
        {
            return await ExecuteWithConnectionAsync(async connection =>
            {
                const string sql = @"
                    SELECT 
                        p.id,
                        p.slug,
                        p.title,
                        p.content,
                        p.content_preview,
                        p.author_id,
                        p.published_at,
                        p.created_at,
                        p.updated_at,
                        p.is_published,
                        u.id AS author_id,
                        u.name
                    FROM posts AS p
                    JOIN users AS u 
                        ON p.author_id = u.id
                    WHERE p.slug = @Slug AND is_published = true
                    ORDER BY p.created_at DESC";

                return await connection.QueryFirstOrDefaultAsync<BlogPost>(sql, new { Slug = slug });
            });
        }

        public async Task<IEnumerable<BlogPost?>> GetByAuthorAsync(string authorName)
        {
            return await ExecuteWithConnectionAsync(async connection =>
            {
                const string sql = @"
                    SELECT 
                        p.id,
                        p.slug,
                        p.title,
                        p.content,
                        p.content_preview,
                        p.author_id,
                        p.published_at,
                        p.created_at,
                        p.updated_at,
                        p.is_published,
                        u.id AS author_id,
                        u.name
                    FROM posts AS p
                    JOIN users AS u 
                        ON p.author_id = u.id
                    WHERE u.name = @AuthorName AND is_published = true
                    ORDER BY p.created_at DESC";

                return await connection.QueryAsync<BlogPost>(sql, new { AuthorName = authorName });
            });
        }

        public async Task<BlogPost?> CreateAsync(BlogPost entity)
        {
            return await ExecuteWithConnectionAsync(async connection =>
            {
                const string sql = @"
                    INSERT INTO posts 
                        (slug, title, content, content_preview, published_at, created_at, author_id, is_published)
                    VALUES 
                        (@Slug, @Title, @Content, @ContentPreview, @PublishedAt, @CreatedAt, @AuthorId, @IsPublished)
                    RETURNING 
                        id, slug, title, content, content_preview, published_at, is_published";

                var parameters = new
                {
                    entity.Slug,
                    entity.Title,
                    entity.Content,
                    entity.ContentPreview,
                    entity.AuthorId,
                    entity.CreatedAt,
                    entity.PublishedAt,
                    entity.IsPublished
                };

                var createdPost = await connection.QuerySingleOrDefaultAsync<BlogPost>(sql, parameters);
                return createdPost;
            });
        }

        public async Task<BlogPost?> UpdateAsync(Guid id, BlogPost entity)
        {
            return await ExecuteWithConnectionAsync(async connection =>
            {
                const string sql = @"
                    UPDATE posts 
                    SET slug = @Slug,
                        title = @Title,
                        content = @Content, 
                        content_preview = @ContentPreview,
                        author_id = @AuthorId, 
                        updated_at = @UpdatedAt, 
                        is_published = @IsPublished
                    WHERE id = @Id
                    RETURNING 
                        id, slug, title, content, content_preview, updated_at, is_published";

                var parameters = new
                {
                    entity.Slug,
                    entity.Title,
                    entity.Content,
                    entity.ContentPreview,
                    entity.AuthorId,
                    entity.UpdatedAt,
                    entity.IsPublished,
                    Id = id
                };

                await connection.ExecuteAsync(sql, parameters);
                return entity;
            });
        }

        public async Task DeleteAsync(Guid id)
        {
            await ExecuteWithConnectionAsync(async connection =>
            {
                const string sql = @"
                    UPDATE posts
                    SET is_published = false,
                        updated_at = @UpdatedAt
                    WHERE id = @Id";

                await connection.ExecuteAsync(sql, new { Id = id, UpdatedAt = DateTime.UtcNow });
            });
        }

        public async Task<bool> SlugExistsAsync(string slug)
        {
            return await ExecuteWithConnectionAsync(async connection =>
            {
                const string sql = @"
                    SELECT COUNT(slug)
                    FROM posts 
                    WHERE slug = @Slug AND is_published = true";

                var count = await connection.ExecuteScalarAsync<int>(sql, new { Slug = slug });
                Console.WriteLine($"[DEBUG] Quantidade de slugs encontradas: {count}");

                return count > 0;
            });
        }
    }
}