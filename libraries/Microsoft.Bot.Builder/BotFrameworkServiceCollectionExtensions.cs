using System;
using Microsoft.Bot.StreamingExtensions;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.Builder
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
