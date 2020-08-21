using System;


namespace Svv.Broadcaster.Configuration
{
    public class BroadcastMessengerConfig
    {
        public int Port { get; set; }
        public string Header { get; set; }
        public TimeSpan AnnouncingInterval { get; set; }
    }
}
