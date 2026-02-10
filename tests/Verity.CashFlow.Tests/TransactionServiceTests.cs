using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Verity.CashFlow.API.Application.DTOs;
using Verity.CashFlow.API.Application.Services;
using Verity.CashFlow.API.Domain;
using Verity.CashFlow.API.Domain.Interfaces;
using Verity.Contracts;

namespace Verity.CashFlow.Tests;

public class TransactionServiceTests
{
    private readonly Mock<ITransactionRepository> _repoMock;
    private readonly Mock<IPublishEndpoint> _publishMock;
    private readonly Mock<ILogger<TransactionService>> _loggerMock;
    private readonly TransactionService _service;

    public TransactionServiceTests()
    {
        _repoMock = new Mock<ITransactionRepository>();
        _publishMock = new Mock<IPublishEndpoint>();
        _loggerMock = new Mock<ILogger<TransactionService>>();

        _service = new TransactionService(_repoMock.Object, _publishMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateTransactionAsync_ShouldPersistAndPublish()
    {
        // Arrange
        var request = new CreateTransactionRequest(150, TransactionType.Debit, "Grocery");
        var userId = "user123";

        // Act
        var result = await _service.CreateTransactionAsync(request, userId);

        // Assert
        result.Should().NotBeNull();
        result.Amount.Should().Be(150);

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        _publishMock.Verify(p => p.Publish(
            It.Is<TransactionCreatedEvent>(e =>
                e.Amount == 150 &&
                e.Description == "Grocery" &&
                e.Type == TransactionType.Debit),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetRecentTransactionsAsync_ShouldReturnMappedDtos()
    {
        // Arrange
        var userId = "user123";
        var transactions = new List<Transaction>
        {
            Transaction.Create(100, TransactionType.Credit, "T1", userId),
            Transaction.Create(50, TransactionType.Debit, "T2", userId)
        };

        _repoMock.Setup(r => r.GetRecentByUserIdAsync(userId, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions);

        // Act
        var result = await _service.GetRecentTransactionsAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.First().Description.Should().Be("T1");
        result.Last().Description.Should().Be("T2");
    }
}
