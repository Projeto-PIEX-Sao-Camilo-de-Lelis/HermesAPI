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
                        p.title, 
                        p.content, 
                        p.created_at, 
                        p.updated_at, 
                        p.is_published, 
                        u.id AS author_id, 
                        u.name AS author_name 
                     FROM posts AS p 
                     JOIN users AS u 
                        ON p.author_id = u.id 
                     WHERE is_published = true 
                     ORDER BY p.created_at DESC";

                return await connection.QueryAsync<Post>(sql);
            });
        }

        public async Task<Post?> GetByIdAsync(Guid id)
        {
            return await ExecuteWithConnectionAsync(async connection =>
            {
                const string sql = @"
                    SELECT 
                        p.title,
                        p.content,
                        p.created_at,
                        p.updated_at,
                        p.is_published,
                        u.id AS author_id,
                        u.name AS author_name 
                    FROM posts AS p 
                    JOIN users AS u 
                        ON p.author_id = u.id 
                    WHERE id = @Id AND is_published = true 
                    ORDER BY p.created_at DESC";

                return await connection.QueryFirstOrDefaultAsync<Post>(sql, new { Id = id });
            });
        }

        public async Task<Post?> GetByAuthorAsync(Guid authorId)
        {
            return await ExecuteWithConnectionAsync(async connection =>
            {
                const string sql = @"
                    SELECT 
                        title,
                        content,
                        created_at,
                        updated_at,
                        author_id,
                        is_published,
                        u.id AS author_id,
                        u.name AS author_name
                    FROM posts AS p
                    JOIN users AS u 
                        ON p.author_id = u.id
                    WHERE author_id = @AuthorId AND is_published = true
                    ORDER BY p.created_at DESC";

                return await connection.QueryFirstOrDefaultAsync<Post>(sql, new { AuthorId = authorId });
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

                var parameters = new {
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

        public async Task<Post?> UpdateAsync(Post entity)
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
                    entity.IsPublished
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