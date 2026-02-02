using Verity.Core.Domain;

namespace Verity.Consolidated.API.Domain;

public class DailyBalance : Entity, IAggregateRoot
{
    public DateTime Date { get; private set; }
    public decimal TotalCredit { get; private set; }
    public decimal TotalDebit { get; private set; }
    public decimal ClosingBalance { get; private set; }

    private DailyBalance() { }

    public DailyBalance(DateTime date)
    {
        Date = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        TotalCredit = 0;
        TotalDebit = 0;
        ClosingBalance = 0;
    }

    public void AddCredit(decimal amount)
    {
        if (amount < 0) throw new ArgumentException("O valor deve ser positivo");
        TotalCredit += amount;
        UpdateClosingBalance();
    }

    public void AddDebit(decimal amount)
    {
        if (amount < 0) throw new ArgumentException("O valor deve ser positivo");
        TotalDebit += amount;
        UpdateClosingBalance();
    }

    private void UpdateClosingBalance()
    {
        ClosingBalance = TotalCredit - TotalDebit;
    }
}
