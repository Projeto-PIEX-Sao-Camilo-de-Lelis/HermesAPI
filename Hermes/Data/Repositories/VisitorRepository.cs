using Dapper;
using Hermes.Core.Interfaces.Data;
using Hermes.Core.Interfaces.Repository;
using Hermes.Core.Models;

namespace Hermes.Data.Repositories;

public class VisitorRepository : BaseRepository, IVisitorRepository
{
    public VisitorRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<int> GetVisitorCountAsync()
    {
        return await ExecuteWithConnectionAsync(async connection =>
        {
            const string sql = "SELECT COUNT(*) FROM visitors";
            return await connection.QuerySingleAsync<int>(sql);
        });
    }

    public async Task<Dictionary<string, int>> GetVisitorsByCountryAsync()
    {
        return await ExecuteWithConnectionAsync(async connection =>
        {
            const string sql = @"
            SELECT country, COUNT(*) as count
            FROM visitors
            GROUP BY country
            ORDER BY count DESC";

            var results = await connection.QueryAsync<(string Country, int Count)>(sql);
            return results.ToDictionary(r => r.Country, r => r.Count);
        });
    }

    public async Task<Dictionary<DateTime, int>> GetVisitorsByDateAsync(DateTime startDate, DateTime endDate)
    {
        return await ExecuteWithConnectionAsync(async connection =>
        {
            const string sql = @"
            -- Extrair data no fuso UTC sem considerar horas
            SELECT DATE_TRUNC('day', visit_date AT TIME ZONE 'UTC') as date, COUNT(*) as count
            FROM visitors
            WHERE visit_date >= @StartDate AND visit_date <= @EndDate
            GROUP BY DATE_TRUNC('day', visit_date AT TIME ZONE 'UTC')
            ORDER BY date";

            var parameters = new { StartDate = startDate, EndDate = endDate };
            var results = await connection.QueryAsync<(DateTime Date, int Count)>(sql, parameters);
            
            return results.ToDictionary(
                r => DateTime.SpecifyKind(r.Date, DateTimeKind.Utc),
                r => r.Count);
        });
    }

    public async Task<Guid> RecordVisitAsync(Visitor visitor)
    {
        return await ExecuteWithConnectionAsync(async connection =>
        {
            const string sql = @"
                    INSERT INTO visitors 
                        (id, ip_address, country, visit_date, page_url)
                    VALUES
                        (@Id, @IpAddress, @Country, @VisitDate, @PageUrl)
                    RETURNING id";

            var id = await connection.QuerySingleAsync<Guid>(sql, visitor);
            return id;
        });
    }

    public async Task<bool> HasVisitorBeenRecordedInPeriodAsync(string ipAddress, DateTime startDate, DateTime endDate)
    {
        return await ExecuteWithConnectionAsync(async connection =>
        {
            const string sql = @"
            SELECT COUNT(*) 
            FROM visitors 
            WHERE ip_address = @IpAddress 
            AND visit_date >= @StartDate 
            AND visit_date < @EndDate";

            var parameters = new { IpAddress = ipAddress, StartDate = startDate, EndDate = endDate };
            var count = await connection.QuerySingleAsync<int>(sql, parameters);

            return count > 0;
        });
    }
}