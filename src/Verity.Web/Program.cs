using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Verity.Web;
using Verity.Web.Services;
using Polly;
using Polly.Extensions.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();

// Autenticação OIDC
builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Local", options.ProviderOptions);
    options.ProviderOptions.ResponseType = "code";
    options.ProviderOptions.DefaultScopes.Add("verity-api");
});

builder.Services.AddScoped<CustomAuthorizationMessageHandler>();
builder.Services.AddScoped<TransactionService>();

// Cliente HTTP para Transações (Serviço CashFlow) - Autenticado
builder.Services.AddHttpClient("CashFlowAPI", client => client.BaseAddress = new Uri("http://localhost:5002/"))
    .AddHttpMessageHandler<CustomAuthorizationMessageHandler>()
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());

// Cliente HTTP para Relatórios (Serviço Consolidated) - Autenticado
builder.Services.AddHttpClient("ConsolidatedAPI", client => client.BaseAddress = new Uri("http://localhost:5003/"))
    .AddHttpMessageHandler<CustomAuthorizationMessageHandler>()
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());

// Cliente HTTP Padrão
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5002/") });

await builder.Build().RunAsync();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
}
