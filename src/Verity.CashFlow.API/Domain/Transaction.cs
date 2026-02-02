using Verity.Contracts;
using Verity.Core.Domain;

namespace Verity.CashFlow.API.Domain;

public class Transaction : Entity, IAggregateRoot
{
    public decimal Amount { get; private set; }
    public TransactionType Type { get; private set; }
    public string Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string UserId { get; private set; }

    private Transaction() { }

    private Transaction(decimal amount, TransactionType type, string description, string userId)
    {
        if (amount <= 0) throw new ArgumentException("O valor deve ser maior que zero.");
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("A descrição é obrigatória.");
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("O ID do usuário é obrigatório.");

        Amount = amount;
        Type = type;
        Description = description;
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
    }

    public static Transaction Create(decimal amount, TransactionType type, string description, string userId)
    {
        return new Transaction(amount, type, description, userId);
    }
}
