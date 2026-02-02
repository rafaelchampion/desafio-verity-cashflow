using MassTransit;
using Verity.Consolidated.API.Domain;
using Verity.Consolidated.API.Infrastructure.Data;
using Verity.Contracts;

namespace Verity.Consolidated.API.Infrastructure.Messaging;

public class TransactionCreatedConsumer : IConsumer<TransactionCreatedEvent>
{
    private readonly DailyBalanceRepository _repository;
    private readonly ILogger<TransactionCreatedConsumer> _logger;

    public TransactionCreatedConsumer(DailyBalanceRepository repository, ILogger<TransactionCreatedConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TransactionCreatedEvent> context)
    {
        var message = context.Message;
        var date = DateTime.SpecifyKind(message.CreatedAt.Date, DateTimeKind.Utc);

        _logger.LogInformation("Processando transação {Id} para a data {Date}", message.TransactionId, date);

        var dailyBalance = await _repository.GetByDateAsync(date);

        if (dailyBalance == null)
        {
            dailyBalance = new DailyBalance(date);
            await _repository.AddAsync(dailyBalance);
        }

        if (message.Type == TransactionType.Credit)
        {
            dailyBalance.AddCredit(message.Amount);
        }
        else
        {
            dailyBalance.AddDebit(message.Amount);
        }

        await _repository.SaveChangesAsync();

        _logger.LogInformation("Saldo atualizado para {Date}. Novo Fechamento: {Balance}", date, dailyBalance.ClosingBalance);
    }
}
