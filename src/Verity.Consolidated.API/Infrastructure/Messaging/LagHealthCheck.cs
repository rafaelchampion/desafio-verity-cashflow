using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Verity.Consolidated.API.Infrastructure.Messaging;

public class LagHealthCheck : IHealthCheck
{
    private readonly ProcessingStatus _status;

    public LagHealthCheck(ProcessingStatus status)
    {
        _status = status;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var lag = DateTime.UtcNow - _status.LastProcessedTime;
        
        if (lag.TotalMinutes > 5)
        {
            return Task.FromResult(HealthCheckResult.Degraded($"O processamento de eventos está atrasado em {lag.TotalMinutes} minutos."));
        }
        
        return Task.FromResult(HealthCheckResult.Healthy("O processamento de eventos está atualizado."));
    }
}
