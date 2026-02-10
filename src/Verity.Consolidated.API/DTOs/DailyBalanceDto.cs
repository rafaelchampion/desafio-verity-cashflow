namespace Verity.Consolidated.API.DTOs;

public class DailyBalanceDto
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal ClosingBalance { get; set; }
}
