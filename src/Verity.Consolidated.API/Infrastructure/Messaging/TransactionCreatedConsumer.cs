using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Verity.Consolidated.API.Domain;
using Verity.Consolidated.API.Infrastructure.Data;
using Verity.Contracts;

namespace Verity.Consolidated.API.Infrastructure.Messaging;

public class TransactionCreatedConsumer : IConsumer<TransactionCreatedEvent>
{
    private readonly DailyBalanceRepository _repository;
    private readonly ILogger<TransactionCreatedConsumer> _logger;
    private readonly ProcessingStatus _status;
    private readonly IDistributedCache _cache;

    public TransactionCreatedConsumer(DailyBalanceRepository repository, ILogger<TransactionCreatedConsumer> logger, ProcessingStatus status, IDistributedCache cache)
    {
        _repository = repository;
        _logger = logger;
        _status = status;
        _cache = cache;
    }

    public async Task Consume(ConsumeContext<TransactionCreatedEvent> context)
    {
        var message = context.Message;
        var date = DateTime.SpecifyKind(message.CreatedAt.Date, DateTimeKind.Utc);

        _logger.LogWarning(">>> PROCESSANDO TRANSAÇÃO: {Id}, DATA: {Date}, VALOR: {Amount}, TIPO: {Type}", message.TransactionId, date, message.Amount, message.Type);

        try
        {
            await ProcessTransaction(message, date);
        }
        catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505")
        {
            _logger.LogWarning(">>> Concorrência detectada para {Date}. Retentando atualização...", date);
            _repository.ChangeTrackerClear();
            await ProcessTransaction(message, date);
        }
        
        var cacheKey = $"daily_report_v2:{date:yyyy-MM-dd}";
        await _cache.RemoveAsync(cacheKey);
        _logger.LogWarning(">>> Cache invalidado para chave: {Key}", cacheKey);

        _status.LastProcessedTime = DateTime.UtcNow;
    }

    private async Task ProcessTransaction(TransactionCreatedEvent message, DateTime date)
    {
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
