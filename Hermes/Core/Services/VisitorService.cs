using System.Text.Json;
using Hermes.Core.Interfaces.Repository;
using Hermes.Core.Interfaces.Service;
using Hermes.Core.Models;

namespace Hermes.Core.Services;

public class VisitorService : IVisitorService
{
    private readonly IVisitorRepository _visitorRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<VisitorService> _logger;
    
    public VisitorService(IVisitorRepository visitorRepository, IHttpClientFactory httpClientFactory, ILogger<VisitorService> logger)
    {
        _visitorRepository = visitorRepository;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }
    
    public async Task<Guid> RecordVisitAsync(string ipAddress, string pageUrl)
    {
        try
        {
            var country = await GetCountryFromIpAsync(ipAddress);
            
            var visitor = new Visitor
            {
                IpAddress = ipAddress,
                Country = country,
                VisitDate = DateTime.UtcNow,
                PageUrl = pageUrl
            };

            return await _visitorRepository.RecordVisitAsync(visitor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar visita para IP {IpAddress}", ipAddress);
            throw;
        }
    }
    
    public Task<int> GetVisitorCountAsync()
    {
        return _visitorRepository.GetVisitorCountAsync();
    }

    public Task<Dictionary<string, int>> GetVisitorsByCountryAsync()
    {
        return _visitorRepository.GetVisitorsByCountryAsync();
    }

    public Task<Dictionary<DateTime, int>> GetVisitorsByDateAsync(DateTime startDate, DateTime endDate)
    {
        return _visitorRepository.GetVisitorsByDateAsync(startDate, endDate);
    }
    
    private async Task<string> GetCountryFromIpAsync(string ipAddress)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"http://ip-api.com/json/{ipAddress}");
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(content);
            
            if (document.RootElement.TryGetProperty("country", out var countryElement))
            {
                return countryElement.GetString() ?? "Desconhecido";
            }
            
            return "Desconhecido";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter país para o IP {IpAddress}", ipAddress);
            return "Desconhecido";
        }
    }
}