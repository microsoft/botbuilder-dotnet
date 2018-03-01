using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;

namespace Microsoft.Bot.Builder.Integration.NetCore
{
    public static class ServiceCollectionExtensions
    {
        private static readonly JsonSerializer ActivitySerializer = JsonSerializer.Create();

        public static IBotFrameworkBuilder AddBotFramework(this IServiceCollection services) => AddBotFramework(services, null);

        public static IBotFrameworkBuilder AddBotFramework(this IServiceCollection services, Action<BotFrameworkOptions> setupAction)
        {
            services.AddRouting();

            return new BotFrameworkBuilder(services);
        }
    }
}
