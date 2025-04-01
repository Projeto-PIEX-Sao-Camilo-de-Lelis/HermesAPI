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
                        u.id AS author_id, 
                        u.name AS author_name 
                     FROM posts p 
                     JOIN users u ON p.author_id = u.id 
                     WHERE is_published = true 
                     ORDER BY p.created_at DESC";

                return await connection.QueryAsync<Post>(sql);
            });
        }

        public async Task<Post?> GetByIdAsync(Guid id)
        {
            return await ExecuteWithConnectionAsync(async connection =>
            {
                const string sql = @"SELECT * FROM posts WHERE id = @Id AND is_published = true";
                return await connection.QueryFirstOrDefaultAsync<Post>(sql, new { Id = id });
            });
        }

        public async Task<Post?> GetByAuthorAsync(Guid authorId)
        {
            return await ExecuteWithConnectionAsync(async connection =>
            {
                const string sql = @"SELECT * FROM posts WHERE author_id = @AuthorId AND is_published = true";
                return await connection.QueryFirstOrDefaultAsync<Post>(sql, new { AuthorId = authorId });
            });
        }

        public async Task<Post?> CreateAsync(Post entity)
        {
            throw new NotImplementedException();
        }

        public async Task<Post?> UpdateAsync(Post entity)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}