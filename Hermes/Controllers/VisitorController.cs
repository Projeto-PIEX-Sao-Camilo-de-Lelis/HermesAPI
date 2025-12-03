using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hermes.Core.Dtos.Requests;
using Hermes.Core.Interfaces.Service;

namespace Hermes.Controllers;

[Route("api/v1/visitors")]
[ApiController]
public class VisitorController : ControllerBase
{
    private readonly IVisitorService _visitorService;
    
    public VisitorController(IVisitorService visitorService)
    {
        _visitorService = visitorService;
    }

    [HttpPost("record")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RecordVisit([FromBody] RecordVisitRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.PageUrl))
        {
            return BadRequest(new { message = "URL da página é obrigatória" });
        }

        try
        {
            string ipAddress = GetClientIpAddress();

            await _visitorService.RecordVisitAsync(ipAddress, request.PageUrl);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao registrar visita" });
        }
    }

    [HttpGet("count")]
    [Authorize]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetVisitorCount()
    {
        var count = await _visitorService.GetVisitorCountAsync();
        return Ok(count);
    }

    [HttpGet("by-country")]
    [Authorize]
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, int>>> GetVisitorsByCountry()
    {
        var visitsByCountry = await _visitorService.GetVisitorsByCountryAsync();
        return Ok(visitsByCountry);
    }

    [HttpGet("by-date")]
    [Authorize]
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, int>>> GetVisitorsByDate(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        if (startDate == default || endDate == default)
        {
            startDate = DateTime.UtcNow.Date;
            endDate = startDate.AddDays(1);
        }
        
        endDate = endDate.Date.AddDays(1).AddMilliseconds(-1);

        if (startDate > endDate)
        {
            return BadRequest(new { message = "A data inicial deve ser anterior à data final" });
        }
        
        var visitsByDate = await _visitorService.GetVisitorsByDateAsync(startDate, endDate);
        
        var result = new Dictionary<string, int>();
        foreach (var entry in visitsByDate)
        {
            var dateKey = entry.Key.ToString("yyyy-MM-dd");
            if (result.ContainsKey(dateKey))
            {
                result[dateKey] += entry.Value;
            }
            else
            {
                result[dateKey] = entry.Value;
            }
        }

        return Ok(result);
    }

    private string GetClientIpAddress()
    {
        string ip = Request.Headers["X-Forwarded-For"].FirstOrDefault() ??
                    Request.Headers["X-Real-IP"].FirstOrDefault();

        if (string.IsNullOrEmpty(ip))
        {
            ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        }
        else
        {
            ip = ip.Split(',')[0].Trim();
        }

        return ip;
    }
}