// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;

namespace Microsoft.Bot.Builder.Integration.NetCore
{
    public static class ServiceCollectionExtensions
    {
        private static readonly JsonSerializer ActivitySerializer = JsonSerializer.Create();

        public static IBotConfigurationBuilder AddBot<TBot>(this IServiceCollection services, Action<BotFrameworkOptions> setupAction = null) where TBot : class, IBot
        {
            services.AddTransient<IBot, TBot>();

            var botBuilder = new BotConfigurationBuilder(services);

            services.Configure<BotFrameworkOptions>(options =>
            {
                options.Middleware.AddRange(botBuilder.Middleware);
            });

            services.Configure(setupAction);

            return botBuilder;
        }
    }
}
