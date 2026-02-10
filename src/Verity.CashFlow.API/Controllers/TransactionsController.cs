using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Verity.CashFlow.API.Application.DTOs;
using Verity.CashFlow.API.Application.Services;

namespace Verity.CashFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly TransactionService _service;
    private readonly IDistributedCache _cache;

    public TransactionsController(TransactionService service, IDistributedCache cache)
    {
        _service = service;
        _cache = cache;
    }

    [HttpPost]
    public async Task<ActionResult<TransactionResponse>> Create(CreateTransactionRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        if (Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey))
        {
            var cacheKey = $"idempotency:{userId}:{idempotencyKey}";
            var cached = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cached))
            {
                return Conflict("Duplicate request processed.");
            }
            
            await _cache.SetStringAsync(cacheKey, "processed", new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });
        }

        try
        {
            var result = await _service.CreateTransactionAsync(request, userId);
            return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TransactionResponse>>> GetRecent()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _service.GetRecentTransactionsAsync(userId);
        return Ok(result);
    }
}
