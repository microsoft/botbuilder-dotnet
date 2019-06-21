using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.StreamingExtensions
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
