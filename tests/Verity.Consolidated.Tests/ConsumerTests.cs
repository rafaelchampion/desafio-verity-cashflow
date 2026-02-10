using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Verity.Consolidated.API.Domain;
using Verity.Consolidated.API.Infrastructure.Data;
using Verity.Consolidated.API.Infrastructure.Messaging;
using Verity.Contracts;

namespace Verity.Consolidated.Tests;

public class ConsumerTests
{
    private readonly ConsolidatedDbContext _dbContext;
    private readonly DailyBalanceRepository _repository;
    private readonly Mock<ILogger<TransactionCreatedConsumer>> _loggerMock;
    private readonly ProcessingStatus _processingStatus;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly TransactionCreatedConsumer _consumer;

    public ConsumerTests()
    {
        var options = new DbContextOptionsBuilder<ConsolidatedDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test
            .Options;

        _dbContext = new ConsolidatedDbContext(options);
        _repository = new DailyBalanceRepository(_dbContext);
        _loggerMock = new Mock<ILogger<TransactionCreatedConsumer>>();
        _processingStatus = new ProcessingStatus();
        _cacheMock = new Mock<IDistributedCache>();
        _consumer = new TransactionCreatedConsumer(_repository, _loggerMock.Object, _processingStatus, _cacheMock.Object);
    }

    [Fact]
    public async Task Consume_ShouldCreateDailyBalance_WhenItDoesNotExist()
    {
        // Arrange
        var contextMock = new Mock<ConsumeContext<TransactionCreatedEvent>>();
        var evt = new TransactionCreatedEvent(Guid.NewGuid(), 100, TransactionType.Credit, DateTime.UtcNow, "Test");
        contextMock.Setup(c => c.Message).Returns(evt);

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        var balance = await _dbContext.DailyBalances.FirstOrDefaultAsync();
        balance.Should().NotBeNull();
        balance!.TotalCredit.Should().Be(100);
        balance.ClosingBalance.Should().Be(100);
        balance.Date.Should().Be(evt.CreatedAt.Date);
    }

    [Fact]
    public async Task Consume_ShouldUpdateExistingBalance()
    {
        // Arrange
        var date = DateTime.UtcNow.Date;
        var initialBalance = new DailyBalance(date);
        initialBalance.AddCredit(50);
        _dbContext.DailyBalances.Add(initialBalance);
        await _dbContext.SaveChangesAsync();

        var contextMock = new Mock<ConsumeContext<TransactionCreatedEvent>>();
        var evt = new TransactionCreatedEvent(Guid.NewGuid(), 30, TransactionType.Debit, DateTime.UtcNow, "Test Debit");
        contextMock.Setup(c => c.Message).Returns(evt);

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        var balance = await _dbContext.DailyBalances.FirstOrDefaultAsync(d => d.Date == date);
        balance.Should().NotBeNull();
        balance!.TotalCredit.Should().Be(50);
        balance.TotalDebit.Should().Be(30);
        balance.ClosingBalance.Should().Be(20); // 50 - 30
    }

    [Fact]
    public async Task Consume_ShouldThrowException_WhenAmountIsNegative()
    {
        // Arrange
        var contextMock = new Mock<ConsumeContext<TransactionCreatedEvent>>();
        var evt = new TransactionCreatedEvent(Guid.NewGuid(), -50, TransactionType.Credit, DateTime.UtcNow, "Invalid");
        contextMock.Setup(c => c.Message).Returns(evt);

        // Act
        Func<Task> act = async () => await _consumer.Consume(contextMock.Object);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("O valor deve ser positivo");
    }
}
