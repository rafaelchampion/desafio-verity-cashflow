using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verity.Consolidated.API.Domain;
using Verity.Consolidated.API.Infrastructure.Data;

namespace Verity.Consolidated.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportController : ControllerBase
{
    private readonly DailyBalanceRepository _repository;

    public ReportController(DailyBalanceRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("daily")]
    public async Task<ActionResult<DailyBalance>> GetDailyReport([FromQuery] DateTime date)
    {
        var report = await _repository.GetByDateReadOnlyAsync(date.Date);
        if (report == null)
        {
            return NotFound("Nenhum dado consolidado para esta data.");
        }

        return Ok(report);
    }
}
