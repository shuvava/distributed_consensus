using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Svv.Application.Api.Extensions
{
    public static class HealthProbeStartup
    {
        public const string ReadinessUrl = "/health/readiness";
        public const string LivenessUrl = "/health/liveness";
                public static IServiceCollection ConfigureHealthProbe(this IServiceCollection services,
            IConfiguration configuration)
        {
            var tags = new[] {"ready"};

            services.AddHealthChecks()
                ;

            return services;
        }

        public static IApplicationBuilder ConfigureHealthProbe(this IApplicationBuilder app)
        {
            app.UseHealthChecks(ReadinessUrl, new HealthCheckOptions
            {
                ResponseWriter = HttpResponseWriter.WriteHealthReadinessResponse,
                Predicate = check => check.Tags.Contains("ready")
            });

            app.UseHealthChecks(LivenessUrl, new HealthCheckOptions
            {
                ResponseWriter = HttpResponseWriter.WriteHealthLivenessResponse,
                Predicate = _ => false
            });

            return app;
        }
    }
}
