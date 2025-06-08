using Hermes.Core.Models;

namespace Hermes.Core.Interfaces.Repository;

public interface IVisitorRepository
{
    Task<Guid> RecordVisitAsync(Visitor visitor);
    Task<int> GetVisitorCountAsync();
    Task<Dictionary<string, int>> GetVisitorsByCountryAsync();
    Task<Dictionary<DateTime, int>> GetVisitorsByDateAsync(DateTime startDate, DateTime endDate);
}