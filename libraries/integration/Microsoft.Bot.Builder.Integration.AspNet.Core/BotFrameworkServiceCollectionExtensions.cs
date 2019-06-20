using System;
using Microsoft.Bot.Protocol.StreamingExtensions.NetCore;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.Protocol.StreamingExtensions
{
    public static partial class BotFrameworkServiceCollectionExtensions
    {
        public static IServiceCollection UseWebSocketAdapter(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<WebSocketEnabledHttpAdapter>();
            return services;
        }
    }
}
