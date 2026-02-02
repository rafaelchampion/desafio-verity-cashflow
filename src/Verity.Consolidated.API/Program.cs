using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Verity.Consolidated.API.Infrastructure.Data;
using Verity.Consolidated.API.Infrastructure.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ConsolidatedDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<DailyBalanceRepository>();

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    x.AddConsumer<TransactionCreatedConsumer>();

    x.AddEntityFrameworkOutbox<ConsolidatedDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMq") ?? "amqp://guest:guest@localhost:5672");

        cfg.ReceiveEndpoint("transaction-created-event", e =>
        {
            e.ConfigureConsumer<TransactionCreatedConsumer>(context);
            e.UseEntityFrameworkOutbox<ConsolidatedDbContext>(context);
            e.UseMessageRetry(r => r.Interval(5, TimeSpan.FromMilliseconds(200)));
        });
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
    var context = scope.ServiceProvider.GetRequiredService<ConsolidatedDbContext>();
    try
    {
        context.Database.Migrate();
    }
    catch (Exception) { }
}

app.MapControllers();

app.Run();
