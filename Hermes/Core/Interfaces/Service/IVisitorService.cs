using Hermes.Core.Models;

namespace Hermes.Core.Interfaces.Service;

public interface IVisitorService
{
    Task<Guid> RecordVisitAsync(string ipAddress, string pageUrl);
    Task<int> GetVisitorCountAsync();
    Task<Dictionary<string, int>> GetVisitorsByCountryAsync();
    Task<Dictionary<DateTime, int>> GetVisitorsByDateAsync(DateTime startDate, DateTime endDate);
}