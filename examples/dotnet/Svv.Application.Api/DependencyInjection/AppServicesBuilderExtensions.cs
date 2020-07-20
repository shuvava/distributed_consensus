using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Svv.Application.Api.BackgroundServices;
using Svv.Broadcaster;
using Svv.Broadcaster.Configuration;


namespace Svv.Application.Api.DependencyInjection
{
    public static class AppServicesBuilderExtensions
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<BroadcastMessengerConfig>(config.GetSection("BroadcastMessenger"));
            services
                .AddSingleton<IBroadcastMessageSender, BroadcastMessenger>()
                .AddSingleton<IBroadcastMessageReceiver, BroadcastMessenger>()
                .AddHostedService<BroadcastMessageReceiverBackgroundService>()
                ;
            var enableSender = config.GetValue<string>("ENABLE_BROADCAST_SENDER");

            if (enableSender != null && enableSender.Equals("true"))
            {
                services.AddHostedService<BroadcastMessageSenderBackgroundService>();
            }
            return services;
        }
    }
}
