namespace Verity.Contracts;

public enum TransactionType
{
    Debit = 0,
    Credit = 1
}

public record TransactionCreatedEvent(
    Guid TransactionId,
    decimal Amount,
    TransactionType Type,
    DateTime CreatedAt,
    string Description
);
