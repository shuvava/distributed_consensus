using Microsoft.Extensions.Diagnostics.HealthChecks;


namespace Svv.Application.UdpBroadcast.Api.Models
{
    public class ReadinessHealthReport
    {
        public string Version { get; set; }
        public HealthStatus Status { get; set;  }
    }
}
