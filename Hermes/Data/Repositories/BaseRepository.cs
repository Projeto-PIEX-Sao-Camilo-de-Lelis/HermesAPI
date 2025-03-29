using System.Data;
using Hermes.Core.Interfaces.Data;

namespace Hermes.Data.Repositories
{
    public abstract class BaseRepository
    {
        protected readonly IDbConnectionFactory _connectionFactory;

        protected BaseRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        protected async Task<T> ExecuteWithConnectionAsync<T>(Func<IDbConnection, Task<T>> action)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                return await action(connection);
            }
        }

        protected async Task ExecuteWithConnectionAsync(Func<IDbConnection, Task> action)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                await action(connection);
            }
        }
    }
}
