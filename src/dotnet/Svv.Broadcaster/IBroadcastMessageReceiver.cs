using System;
using System.Threading;
using System.Threading.Tasks;


namespace Svv.Broadcaster
{
    public interface IBroadcastMessageReceiver
    {
        Task ListeningAsync(Action<string, string> callBackt, CancellationToken token = default);
    }
}
