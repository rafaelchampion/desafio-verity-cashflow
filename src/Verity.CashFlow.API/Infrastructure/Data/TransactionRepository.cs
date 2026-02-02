using Microsoft.EntityFrameworkCore;
using Verity.CashFlow.API.Domain;

using Verity.CashFlow.API.Domain.Interfaces;
namespace Verity.CashFlow.API.Infrastructure.Data;

public class TransactionRepository : ITransactionRepository
{
    private readonly CashFlowDbContext _context;

    public TransactionRepository(CashFlowDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetRecentByUserIdAsync(string userId, int count, CancellationToken cancellationToken = default)
    {
        // ⚡ Bolt: Added AsNoTracking() for read-only query performance
        return await _context.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Transaction entity, CancellationToken cancellationToken = default)
    {
        await _context.Transactions.AddAsync(entity, cancellationToken);
    }

    public Task UpdateAsync(Transaction entity, CancellationToken cancellationToken = default)
    {
        _context.Transactions.Update(entity);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
