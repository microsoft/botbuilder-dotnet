using Microsoft.Extensions.DependencyInjection;
using System;

namespace Microsoft.Bot.Protocol.StreamingExtensions
{
    public static partial class BotFrameworkServiceCollectionExtensions
    {
        public static IServiceCollection AddNamedPipeConnector(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var connector = new NamedPipeConnector();

            services.AddSingleton(connector);

            return services;
        }
    }
}
