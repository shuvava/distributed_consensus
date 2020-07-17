using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Svv.Broadcaster.Configuration;


namespace Svv.Broadcaster
{
    public class BroadcastMessenger : IBroadcastMessageSender, IBroadcastMessageReceiver, IDisposable
    {
        private readonly ILogger<BroadcastMessenger> _logger;
        private readonly BroadcastMessengerConfig _config;
        private readonly CancellationTokenSource _globalToken;
        private static readonly Action<ILogger, string, Exception> BroadcastError = LoggerMessage.Define<string>(
        LogLevel.Error,
        new EventId(5, nameof(BroadcastMessenger)),
            "Exception (Operation = '{operation}')");


        public BroadcastMessenger(ILogger<BroadcastMessenger> logger, IOptions<BroadcastMessengerConfig> config)
        {
            _logger = logger?? throw new ArgumentException(nameof(logger));
            _config = config?.Value ?? throw new ArgumentException(nameof(config));
            _globalToken = new CancellationTokenSource();
        }


        public void Dispose()
        {
            _globalToken.Cancel();
            _globalToken.Dispose();
        }


        public async Task AnnouncingAsync(string message, CancellationToken token = default)
        {

            var bytes = Encoding.ASCII.GetBytes(_config.Header+message);
            var announcingEndpoint = new IPEndPoint(IPAddress.Broadcast, _config.Port);
            using var client = new UdpClient();
            client.ExclusiveAddressUse = false;
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(_globalToken.Token, token);
            while (!cts.IsCancellationRequested)
            {
                try
                {
                    await client.SendAsync(bytes, bytes.Length, announcingEndpoint);
                    await Task.Delay(_config.AnnouncingInterval, cts.Token);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(nameof(AnnouncingAsync)+" exception: {0}", ex.Message);
                    BroadcastError(_logger, nameof(AnnouncingAsync), ex);
                }
            }
            client.Close();
        }


        public async Task ListeningAsync(Action<string, string> callBack, CancellationToken token=default)
        {
            var broadcastEndpoint = new IPEndPoint(IPAddress.Any, _config.Port);
            using var client = new UdpClient(broadcastEndpoint);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(_globalToken.Token, token);

            while (!cts.IsCancellationRequested)
            {
                try
                {
                    var result = await client.ReceiveAsync();
                    var message = Encoding.UTF8.GetString(result.Buffer);

                    if (message.StartsWith(_config.Header))
                    {
                        var data = message.Substring(_config.Header.Length);
                        var hostname = result.RemoteEndPoint.Address.ToString();
                        callBack(hostname, data);
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError(nameof(ListeningAsync)+" exception: {0}", ex.Message);
                    BroadcastError(_logger, nameof(ListeningAsync), ex);
                }
            }
            client.Close();
        }
    }
}
