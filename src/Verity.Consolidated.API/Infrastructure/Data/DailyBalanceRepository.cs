using Microsoft.EntityFrameworkCore;
using Verity.Consolidated.API.Domain;
using Verity.Core.Domain;

namespace Verity.Consolidated.API.Infrastructure.Data;

public class DailyBalanceRepository : IRepository<DailyBalance>
{
    private readonly ConsolidatedDbContext _context;

    public DailyBalanceRepository(ConsolidatedDbContext context)
    {
        _context = context;
    }

    public async Task<DailyBalance?> GetByDateAsync(DateTime date, CancellationToken ct = default)
    {
        var utcDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        return await _context.DailyBalances.FirstOrDefaultAsync(d => d.Date == utcDate, ct);
    }

    /// <summary>
    /// Recupera o saldo consolidado para uma data específica sem rastreamento de alterações (Read-Only).
    /// Otimizado para cenários de leitura onde não haverá modificação da entidade.
    /// </summary>
    public async Task<DailyBalance?> GetByDateReadOnlyAsync(DateTime date, CancellationToken ct = default)
    {
        var utcDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        return await _context.DailyBalances.AsNoTracking().FirstOrDefaultAsync(d => d.Date == utcDate, ct);
    }

    public async Task<DailyBalance?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.DailyBalances.FindAsync(new object[] { id }, ct);
    }

    public async Task AddAsync(DailyBalance entity, CancellationToken ct = default)
    {
        await _context.DailyBalances.AddAsync(entity, ct);
    }

    public Task UpdateAsync(DailyBalance entity, CancellationToken ct = default)
    {
        _context.DailyBalances.Update(entity);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
    public void ChangeTrackerClear()
    {
        _context.ChangeTracker.Clear();
    }
}
