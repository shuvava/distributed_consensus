using System.Threading;
using System.Threading.Tasks;


namespace Svv.Broadcaster
{
    public interface IBroadcastMessageSender
    {
        Task AnnouncingAsync(string message, CancellationToken token = default);
    }
}
