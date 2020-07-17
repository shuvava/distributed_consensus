using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using NUnit.Framework;

using Svv.Broadcaster.Configuration;


#pragma warning disable 4014


namespace Svv.Broadcaster.Tests
{
    [TestFixture]
    public class BroadcastMessengerTest
    {
        [SetUp]
        public void Setup()
        {
        }


        private IOptions<BroadcastMessengerConfig> CreateConfig()
        {
            return Options.Create(new BroadcastMessengerConfig
            {
                Port = 15000,
                Header = nameof(BroadcastMessenger),
                AnnouncingInterval = TimeSpan.FromSeconds(1)
            });
        }
        [Test]
        public async Task SendReceiveFlow()
        {
            var config = CreateConfig();
            var logger = new Mock<ILogger<BroadcastMessenger>>().Object;
            using var client = new BroadcastMessenger(logger, config);
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            var msg = "test";
            var cnt = 0;
            client.AnnouncingAsync(msg, token: token);

            client.ListeningAsync((ip, body) =>
            {
                if (msg.Equals(body))
                {
                    if (cnt == 3)
                    {
                        cts.Cancel();
                    }
                    else
                    {
                        cnt++;
                    }
                }
            }, token: token);

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), token);
            }
            catch (Exception)
            {
                // ignored
            }

            cts.Cancel();
            Assert.AreEqual(3, cnt);
        }
    }
}
