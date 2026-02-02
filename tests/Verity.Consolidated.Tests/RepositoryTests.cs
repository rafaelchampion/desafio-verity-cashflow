using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Verity.Consolidated.API.Domain;
using Verity.Consolidated.API.Infrastructure.Data;

namespace Verity.Consolidated.Tests;

public class RepositoryTests
{
    private readonly ConsolidatedDbContext _dbContext;
    private readonly DailyBalanceRepository _repository;

    public RepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ConsolidatedDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ConsolidatedDbContext(options);
        _repository = new DailyBalanceRepository(_dbContext);
    }

    [Fact]
    public async Task GetByDateReadOnlyAsync_ShouldReturnDetachedEntity()
    {
        // Arrange
        var date = DateTime.UtcNow.Date;
        var entity = new DailyBalance(date);
        _dbContext.DailyBalances.Add(entity);
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByDateReadOnlyAsync(date);

        // Assert
        result.Should().NotBeNull();
        _dbContext.Entry(result!).State.Should().Be(EntityState.Detached);
    }

    [Fact]
    public async Task GetByDateAsync_ShouldReturnUnchangedEntity()
    {
        // Arrange
        var date = DateTime.UtcNow.Date.AddDays(1);
        var entity = new DailyBalance(date);
        _dbContext.DailyBalances.Add(entity);
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByDateAsync(date);

        // Assert
        result.Should().NotBeNull();
        _dbContext.Entry(result!).State.Should().Be(EntityState.Unchanged);
    }
}
