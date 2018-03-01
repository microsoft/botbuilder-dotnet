using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;

namespace Microsoft.Bot.Builder.Integration.NetCore
{
    public static class ServiceCollectionExtensions
    {
        private static readonly JsonSerializer ActivitySerializer = JsonSerializer.Create();

        public static IBotBuilder AddBot<TBot>(this IServiceCollection services, Action<BotFrameworkOptions> setupAction = null) where TBot : class, IBot
        {
            services.AddTransient<IBot, TBot>();

            var botBuilder = new BotBuilder(services);

            services.Configure<BotFrameworkOptions>(options =>
            {
                options.Middleware.AddRange(botBuilder.Middleware);
            });

            services.Configure(setupAction);

            return botBuilder;
        }
    }
}
