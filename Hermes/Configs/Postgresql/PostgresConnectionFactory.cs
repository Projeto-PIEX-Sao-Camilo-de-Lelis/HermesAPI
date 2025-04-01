﻿using Dapper;
using System.Data;
using Hermes.Core.Interfaces.Data;
using Hermes.Data.TypeHandlers;
using Npgsql;

namespace Hermes.Configs.Postgresql
{
    public class PostgresConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public PostgresConnectionFactory(string connectionString)
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString)
            {
                Pooling = true,
                MinPoolSize = 1,
                MaxPoolSize = 20,
                ConnectionIdleLifetime = 300,
                ConnectionPruningInterval = 60,
                Timeout = 15,
                CommandTimeout = 30,
                KeepAlive = 60,
                SslMode = SslMode.Require
            };

            _connectionString = builder.ToString();
        }

        public IDbConnection CreateConnection()
        {
            var connection = new NpgsqlConnection(_connectionString);

            try
            {
                connection.Open();
                SqlMapper.AddTypeHandler(new RoleTypeHandler());
                DefaultTypeMap.MatchNamesWithUnderscores = true;
            }
            catch (NpgsqlException ex)
            {
                throw new NpgsqlException("Não foi possível estabelecer a conexão com a base de dados.", ex);
            }

            return connection;
        }
    }
}