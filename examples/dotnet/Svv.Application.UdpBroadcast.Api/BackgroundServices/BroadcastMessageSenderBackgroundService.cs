using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Svv.Broadcaster;


namespace Svv.Application.UdpBroadcast.Api.BackgroundServices
{
    public class BroadcastMessageSenderBackgroundService: IHostedService
    {
        private readonly CancellationTokenSource _globalToken;
        private readonly ILogger<BroadcastMessageSenderBackgroundService> _logger;
        private readonly IBroadcastMessageSender _broadcastMessageSender;


        public BroadcastMessageSenderBackgroundService(ILogger<BroadcastMessageSenderBackgroundService> logger,
            IBroadcastMessageSender broadcastMessageSender)
        {
            _logger = logger;
            _broadcastMessageSender = broadcastMessageSender;
            _globalToken = new CancellationTokenSource();
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() => _broadcastMessageSender.AnnouncingAsync("test", _globalToken.Token));
            _logger.LogInformation($"{nameof(BroadcastMessageSenderBackgroundService)} Started");
            return Task.CompletedTask;
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(BroadcastMessageSenderBackgroundService)} Stopped");
            _globalToken.Cancel();
            return Task.CompletedTask;
        }
    }
}
