using MassTransit;
using Verity.CashFlow.API.Application.DTOs;
using Verity.CashFlow.API.Domain;
using Verity.CashFlow.API.Domain.Interfaces;
using Verity.Contracts;

namespace Verity.CashFlow.API.Application.Services;

public class TransactionService
{
    private readonly ITransactionRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;

    public TransactionService(ITransactionRepository repository, IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<TransactionResponse> CreateTransactionAsync(CreateTransactionRequest request, string userId)
    {
        var transaction = Transaction.Create(request.Amount, request.Type, request.Description, userId);

        await _repository.AddAsync(transaction);
        
        await _publishEndpoint.Publish(new TransactionCreatedEvent(
            transaction.Id,
            transaction.Amount,
            transaction.Type,
            transaction.CreatedAt,
            transaction.Description
        ));

        await _repository.SaveChangesAsync();

        return new TransactionResponse(transaction.Id, transaction.Amount, transaction.Type, transaction.Description, transaction.CreatedAt);
    }

    public async Task<IEnumerable<TransactionResponse>> GetRecentTransactionsAsync(string userId, int count = 20)
    {
        var transactions = await _repository.GetRecentByUserIdAsync(userId, count);
        return transactions.Select(t => new TransactionResponse(t.Id, t.Amount, t.Type, t.Description, t.CreatedAt));
    }
}
