using FluentAssertions;
using Verity.Consolidated.API.Domain;

namespace Verity.Consolidated.Tests;

public class DailyBalanceDomainTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithZeroValues()
    {
        // Arrange
        var date = DateTime.Now;

        // Act
        var sut = new DailyBalance(date);

        // Assert
        sut.Date.Should().Be(date.Date.ToUniversalTime());
        sut.TotalCredit.Should().Be(0);
        sut.TotalDebit.Should().Be(0);
        sut.ClosingBalance.Should().Be(0);
    }

    [Fact]
    public void AddCredit_ShouldIncreaseTotalCredit_AndUpdateClosingBalance()
    {
        // Arrange
        var sut = new DailyBalance(DateTime.Now);

        // Act
        sut.AddCredit(100);

        // Assert
        sut.TotalCredit.Should().Be(100);
        sut.TotalDebit.Should().Be(0);
        sut.ClosingBalance.Should().Be(100);
    }

    [Fact]
    public void AddDebit_ShouldIncreaseTotalDebit_AndUpdateClosingBalance()
    {
        // Arrange
        var sut = new DailyBalance(DateTime.Now);

        // Act
        sut.AddDebit(50);

        // Assert
        sut.TotalCredit.Should().Be(0);
        sut.TotalDebit.Should().Be(50);
        sut.ClosingBalance.Should().Be(-50);
    }

    [Fact]
    public void MixedOperations_ShouldCalculateCorrectBalance()
    {
        // Arrange
        var sut = new DailyBalance(DateTime.Now);

        // Act
        sut.AddCredit(100); // Bal: 100
        sut.AddDebit(30);   // Bal: 70
        sut.AddCredit(20);  // Bal: 90
        sut.AddDebit(10);   // Bal: 80

        // Assert
        sut.TotalCredit.Should().Be(120);
        sut.TotalDebit.Should().Be(40);
        sut.ClosingBalance.Should().Be(80);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void AddCredit_ShouldThrow_WhenAmountIsNegative(decimal amount)
    {
        // Arrange
        var sut = new DailyBalance(DateTime.Now);

        // Act
        Action act = () => sut.AddCredit(amount);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("O valor deve ser positivo");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void AddDebit_ShouldThrow_WhenAmountIsNegative(decimal amount)
    {
        // Arrange
        var sut = new DailyBalance(DateTime.Now);

        // Act
        Action act = () => sut.AddDebit(amount);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("O valor deve ser positivo");
    }
}
