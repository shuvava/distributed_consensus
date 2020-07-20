using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Svv.Broadcaster;


namespace Svv.Application.Api.BackgroundServices
{
    public class BroadcastMessageReceiverBackgroundService: IHostedService
    {
        private readonly ILogger<BroadcastMessageReceiverBackgroundService> _logger;
        private readonly IBroadcastMessageReceiver _broadcastMessageReceiver;
        private readonly CancellationTokenSource _globalToken;
        public BroadcastMessageReceiverBackgroundService(
            ILogger<BroadcastMessageReceiverBackgroundService> logger,
            IBroadcastMessageReceiver broadcastMessageReceiver)
        {
            _logger = logger;
            _broadcastMessageReceiver = broadcastMessageReceiver;
            _globalToken = new CancellationTokenSource();
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() => _broadcastMessageReceiver.ListeningAsync((ip, body) =>
            {
                _logger.LogInformation($"IP {ip} => message: {body}");
            }, _globalToken.Token));
            _logger.LogInformation($"{nameof(BroadcastMessageReceiverBackgroundService)} Started");
            return Task.CompletedTask;
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(BroadcastMessageReceiverBackgroundService)} Stopped");
            _globalToken.Cancel();
            return Task.CompletedTask;
        }
    }
}
