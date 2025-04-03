using Dapper;
using Hermes.Core.Interfaces.Data;
using Hermes.Core.Interfaces.Repository;
using Hermes.Core.Models;

namespace Hermes.Data.Repositories
{
    public class PostRepository : BaseRepository, IPostRepository
    {
        public PostRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory) { }

        public async Task<IEnumerable<Post?>> GetAllAsync()
        {
            return await ExecuteWithConnectionAsync(async connection =>
            {
                const string sql = @"
                    SELECT 
                        p.id,
                        p.title, 
                        p.content, 
                        p.created_at, 
                        p.updated_at, 
                        p.is_published, 
                        u.id AS author_id, 
                        u.name AS author 
                     FROM posts AS p 
                     JOIN users AS u 
                        ON p.author_id = u.id 
                     WHERE is_published = true 
                     ORDER BY p.created_at DESC";

                return await connection.QueryAsync<Post>(sql);
            });
        }

        public async Task<(IEnumerable<Post> Posts, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
        {
            var offset = (pageNumber - 1) * pageSize;

            const string postsSql = @"
                SELECT 
                    p.id,
                    p.title,
                    p.content,
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

            IEnumerable<Post> posts = Enumerable.Empty<Post>();
            int totalCount = 0;

            await ExecuteWithConnectionAsync(async connection =>
            {
                posts = await connection.QueryAsync<Post>(postsSql, new { PageSize = pageSize, Offset = offset });
            });

            await ExecuteWithConnectionAsync(async connection =>
            {
                totalCount = await connection.ExecuteScalarAsync<int>(countSql);
            });

            return (posts, totalCount);
        }

        public async Task<Post?> GetByIdAsync(Guid id)
        {
            return await ExecuteWithConnectionAsync(async connection =>
            {
                const string sql = @"
                    SELECT 
                        p.id,
                        p.title,
                        p.content,
                        p.author_id,
                        p.created_at,
                        p.updated_at,
                        p.is_published,
                        u.name AS author 
                    FROM posts AS p 
                    JOIN users AS u 
                        ON p.author_id = u.id 
                    WHERE p.id = @Id AND is_published = true 
                    ORDER BY p.created_at DESC";

                return await connection.QueryFirstOrDefaultAsync<Post>(sql, new { Id = id });
            });
        }

        public async Task<IEnumerable<Post?>> GetByAuthorAsync(string authorName)
        {
            return await ExecuteWithConnectionAsync(async connection =>
            {
                const string sql = @"
                    SELECT 
                        p.id,
                        p.title,
                        p.content,
                        p.created_at,
                        p.updated_at,
                        p.author_id,
                        p.is_published,
                        u.id AS author_id,
                        u.name
                    FROM posts AS p
                    JOIN users AS u 
                        ON p.author_id = u.id
                    WHERE u.name = @AuthorName AND is_published = true
                    ORDER BY p.created_at DESC";

                return await connection.QueryAsync<Post>(sql, new { AuthorName = authorName });
            });
        }

        public async Task<Post?> CreateAsync(Post entity)
        {
            return await ExecuteWithConnectionAsync(async connection =>
            {
                entity.CreatedAt = DateTime.UtcNow;

                const string sql = @"
                    INSERT INTO posts 
                        (title, content, created_at, author_id, is_published)
                    VALUES 
                        (@Title, @Content, @CreatedAt, @AuthorId, @IsPublished)
                    RETURNING 
                        id, title, content, author_id, is_published";

                var parameters = new
                {
                    entity.Title,
                    entity.Content,
                    entity.CreatedAt,
                    entity.AuthorId,
                    entity.IsPublished
                };

                var createdPost = await connection.QuerySingleOrDefaultAsync<Post>(sql, parameters);
                return createdPost;
            });
        }

        public async Task<Post?> UpdateAsync(Guid id, Post entity)
        {
            return await ExecuteWithConnectionAsync(async connection =>
            {
                entity.UpdatedAt = DateTime.UtcNow;

                const string sql = @"
                    UPDATE posts 
                    SET title = @Title,
                        content = @Content, 
                        updated_at = @UpdatedAt, 
                        author_id = @AuthorId, 
                        is_published = @IsPublished
                    WHERE id = @Id
                    RETURNING 
                        id, title, content, updated_at, author_id, is_published";

                var parameters = new
                {
                    entity.Title,
                    entity.Content,
                    entity.UpdatedAt,
                    entity.AuthorId,
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
    }
}