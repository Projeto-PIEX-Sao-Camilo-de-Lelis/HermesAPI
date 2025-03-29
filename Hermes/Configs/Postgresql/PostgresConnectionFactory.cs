using System.Data;
using Hermes.Core.Interfaces.Data;
using Npgsql;

namespace Hermes.Configs.Postgresql
{
    public class PostgresConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public PostgresConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection CreateConnection()
        {
            var connection = new NpgsqlConnection(_connectionString);

            try
            {
                connection.Open();
            }
            catch (NpgsqlException)
            {
                throw new NpgsqlException("Não foi possível estabelecer a conexão com a base de dados.");
            }
     
            return connection;
        }
    }
}