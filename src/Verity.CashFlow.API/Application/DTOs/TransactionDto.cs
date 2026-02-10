using System.ComponentModel.DataAnnotations;
using Verity.Contracts;

namespace Verity.CashFlow.API.Application.DTOs;

public record CreateTransactionRequest
(
    [Required(ErrorMessage = "O valor é obrigatório.")] [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero.")] decimal Amount,
    [Required(ErrorMessage = "O tipo é obrigatório.")] [EnumDataType(typeof(TransactionType), ErrorMessage = "Tipo de transação inválido.")] TransactionType Type,
    [Required(ErrorMessage = "A descrição é obrigatória.")] string Description
);

public record TransactionResponse
(
    Guid Id,
    decimal Amount,
    TransactionType Type,
    string Description,
    DateTime CreatedAt
);
