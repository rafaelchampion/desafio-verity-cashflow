using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Verity.CashFlow.API.Application.Services;
using Verity.CashFlow.API.Domain;
using Verity.CashFlow.API.Domain.Interfaces;
using Verity.CashFlow.API.Infrastructure.Data;
using Verity.Core.Domain;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<CashFlowDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IRepository<Transaction>>(provider => provider.GetRequiredService<ITransactionRepository>());
builder.Services.AddScoped<TransactionService>();

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    x.AddEntityFrameworkOutbox<CashFlowDbContext>(o =>
    {
        o.QueryDelay = TimeSpan.FromSeconds(1);
        o.UsePostgres();
        o.UseBusOutbox();
    });

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMq") ?? "amqp://guest:guest@localhost:5672");
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Jwt:Authority"] ?? "http://localhost:8180/realms/verity";
        options.Audience = builder.Configuration["Jwt:Audience"] ?? "account";
        options.RequireHttpsMetadata = false; // Apenas ambiente de desenvolvimento
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.Zero,
            RequireSignedTokens = false,
            ValidateIssuerSigningKey = false,
            SignatureValidator = delegate (string token, TokenValidationParameters parameters)
            {
                var jwt = new Microsoft.IdentityModel.JsonWebTokens.JsonWebToken(token);
                return jwt;
            }
        };
        
        // Crítico: Configurar manualmente para impedir que a API tente se conectar ao Keycloak
        // Isso previne o erro de conexão "Split Horizon" e falhas de DNS no Docker
        options.Configuration = new Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration
        {
            Issuer = "http://localhost:8180/realms/verity"
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CashFlowDbContext>();
    try
    {
        context.Database.Migrate();
    }
    catch(Exception ex)
    {
    }
}

app.MapControllers();

app.Run();
