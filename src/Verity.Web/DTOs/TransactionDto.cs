using System.ComponentModel.DataAnnotations;
using Verity.Contracts;

namespace Verity.Web.DTOs;

public record CreateTransactionRequest
{
    [Required(ErrorMessage = "O valor é obrigatório.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "O tipo é obrigatório.")]
    public TransactionType Type { get; set; } = TransactionType.Credit;

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    public string Description { get; set; } = string.Empty;
}

public record TransactionResponse(
    Guid Id,
    decimal Amount,
    TransactionType Type,
    string Description,
    DateTime CreatedAt
);
