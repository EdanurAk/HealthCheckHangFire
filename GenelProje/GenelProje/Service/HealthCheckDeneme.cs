using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GenelProje.Service
{
    public class HealthCheckDeneme : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            bool isHealthy = true;

            if (isHealthy)
            {
                return Task.FromResult(HealthCheckResult.Healthy("Servis çalışıyor."));
            }
            else
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Servis sağlıksız durumda."));
            }
        }
    }
}
