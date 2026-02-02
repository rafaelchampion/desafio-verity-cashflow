using System.Net.Http.Json;
using Verity.Web.DTOs;

namespace Verity.Web.Services;

public class TransactionService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public TransactionService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IEnumerable<TransactionResponse>> GetRecentTransactionsAsync()
    {
        var client = _httpClientFactory.CreateClient("CashFlowAPI");
        try 
        {
            return await client.GetFromJsonAsync<IEnumerable<TransactionResponse>>("api/transactions") 
                   ?? Enumerable.Empty<TransactionResponse>();
        }
        catch
        {
            return Enumerable.Empty<TransactionResponse>();
        }
    }

    public async Task<bool> CreateTransactionAsync(CreateTransactionRequest request)
    {
        var client = _httpClientFactory.CreateClient("CashFlowAPI");
        var response = await client.PostAsJsonAsync("api/transactions", request);
        return response.IsSuccessStatusCode;
    }
}
