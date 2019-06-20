using System;
using Microsoft.Bot.StreamingExtensions.StreamingExtensions.NetCore;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.StreamingExtensions.StreamingExtensions
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
