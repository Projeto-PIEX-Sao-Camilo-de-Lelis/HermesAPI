﻿using Dapper;
using Hermes.Core.Interfaces.Data;
using Hermes.Core.Interfaces.Repository;
using Hermes.Core.Models;

namespace Hermes.Data.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        public UserRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

        public async Task<IEnumerable<User?>> GetAllAsync()
        {
            return await ExecuteWithConnectionAsync(async connection =>
            {
                const string sql = @"
                    SELECT u.id,
                           u.name,
                           u.email,
                           u.password,
                           u.role,
                           u.created_at,
                           u.updated_at,
                           u.is_active
                    FROM users AS u 
                    WHERE is_active = true";

                return await connection.QueryAsync<User>(sql);
            });

        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await ExecuteWithConnectionAsync(async connection =>
            {
                const string sql = @"
                    SELECT u.id,
                           u.name,
                           u.email,
                           u.password,
                           u.role,
                           u.created_at,
                           u.updated_at,
                           u.is_active
                    FROM users AS u 
                    WHERE u.id = @Id AND is_active = true";

                return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
            });
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await ExecuteWithConnectionAsync(async connection =>
            {
                const string sql = @"
                    SELECT u.id,
                           u.name,
                           u.email,
                           u.password,
                           u.role,
                           u.created_at,
                           u.updated_at,
                           u.is_active 
                    FROM users AS u
                    WHERE email = @Email AND is_active = true";

                return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
            });
        }

        public async Task<User?> CreateAsync(User entity)
        {
            return await ExecuteWithConnectionAsync(async connection =>
            {
                const string sql = @"
                    INSERT INTO users 
                        (name, email, password, role, created_at, is_active) 
                    VALUES 
                        (@Name, @Email, @Password, @Role::role, @CreatedAt, @IsActive) 
                    RETURNING 
                        id, name, email, role, created_at, is_active";

                var parameters = new
                {
                    entity.Name,
                    entity.Email,
                    entity.Password,
                    Role = entity.Role.ToString(),
                    entity.CreatedAt,
                    entity.IsActive
                };

                var user = await connection.QuerySingleOrDefaultAsync<User>(sql, parameters);
                return user;
            });
        }

        public async Task<User?> UpdateAsync(Guid id, User entity)
        {
            return await ExecuteWithConnectionAsync(async connection =>
            {
                const string sql = @"
                    UPDATE users
                    SET name = @Name, 
                        email = @Email, 
                        password = @Password,
                        role = @Role::role,
                        updated_at = @UpdatedAt, 
                        is_active = @IsActive
                    WHERE id = @Id
                    RETURNING 
                        id, name, email, role, updated_at, is_active";

                var parameters = new
                {
                    entity.Name,
                    entity.Email,
                    entity.Password,
                    Role = entity.Role.ToString(),
                    entity.UpdatedAt,
                    entity.IsActive,
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
                    UPDATE users
                    SET is_active = false, 
                        updated_at = @UpdatedAt
                    WHERE id = @Id";

                await connection.ExecuteAsync(sql, new { Id = id, UpdatedAt = DateTime.UtcNow });
            });
        }
    }
}
