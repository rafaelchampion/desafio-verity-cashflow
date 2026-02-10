using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Verity.Consolidated.API.Domain;
using Verity.Consolidated.API.Infrastructure.Data;

namespace Verity.Consolidated.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportController : ControllerBase
{
    private readonly DailyBalanceRepository _repository;
    private readonly IDistributedCache _cache;

    public ReportController(DailyBalanceRepository repository, IDistributedCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    [HttpGet("daily")]
    public async Task<ActionResult<DailyBalance>> GetDailyReport([FromQuery] DateTime date)
    {
        Response.Headers.Append("X-Server-Name", Environment.MachineName);
        var cacheKey = $"daily_report:{date.Date:yyyy-MM-dd}";
        var cached = await _cache.GetStringAsync(cacheKey);
        
        if (!string.IsNullOrEmpty(cached))
        {
             Console.WriteLine($"[ReportController] Retornando dados em cache para {date.Date}: {cached}");
             return Ok(JsonSerializer.Deserialize<DailyBalance>(cached));
        }

        var report = await _repository.GetByDateReadOnlyAsync(date.Date);
        if (report == null)
        {
            Console.WriteLine($"[ReportController] Nenhum dado consolidado para esta data: {date.Date}");
            return NotFound("Nenhum dado consolidado para esta data.");
        }

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = date.Date < DateTime.UtcNow.Date ? TimeSpan.FromHours(1) : TimeSpan.FromMinutes(1)
        };
        
        var json = JsonSerializer.Serialize(report);
        Console.WriteLine($"[ReportController] Retornando dados do banco de dados para {date.Date}: {json}");
        await _cache.SetStringAsync(cacheKey, json, options);

        return Ok(report);
    }
}
