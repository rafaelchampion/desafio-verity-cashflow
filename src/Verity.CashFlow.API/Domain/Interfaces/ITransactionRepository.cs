using Verity.Core.Domain;

namespace Verity.CashFlow.API.Domain.Interfaces;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<IEnumerable<Transaction>> GetRecentByUserIdAsync(string userId, int count, CancellationToken cancellationToken = default);
}
