using FluentAssertions;
using Verity.CashFlow.API.Domain;
using Verity.Contracts;

namespace Verity.CashFlow.Tests;

public class TransactionDomainTests
{
    [Fact]
    public void Create_ShouldReturnTransaction_WhenDataIsValid()
    {
        // Arrange
        decimal amount = 100;
        var type = TransactionType.Credit;
        var desc = "Salary";
        var userId = "user1";

        // Act
        var transaction = Transaction.Create(amount, type, desc, userId);

        // Assert
        transaction.Should().NotBeNull();
        transaction.Amount.Should().Be(amount);
        transaction.Type.Should().Be(type);
        transaction.Description.Should().Be(desc);
        transaction.UserId.Should().Be(userId);
        transaction.Id.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Create_ShouldThrowException_WhenAmountIsInvalid(decimal amount)
    {
        // Act
        Action act = () => Transaction.Create(amount, TransactionType.Credit, "Valid", "user1");

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*O valor*");
    }

    [Fact]
    public void Create_ShouldThrowException_WhenDescriptionIsMissing()
    {
         // Act
        Action act = () => Transaction.Create(100, TransactionType.Credit, "", "user1");

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*A descrição*");
    }

    [Fact]
    public void Create_ShouldThrowException_WhenUserIdIsMissing()
    {
        // Act
        Action act = () => Transaction.Create(100, TransactionType.Credit, "Valid", "");

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*O ID do usuário*");
    }
}
