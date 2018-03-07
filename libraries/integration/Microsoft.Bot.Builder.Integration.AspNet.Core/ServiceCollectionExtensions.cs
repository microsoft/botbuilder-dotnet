// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    public static class ServiceCollectionExtensions
    {
        private static readonly JsonSerializer ActivitySerializer = JsonSerializer.Create();

        public static IServiceCollection AddBot<TBot>(this IServiceCollection services, Action<BotFrameworkOptions> setupAction = null) where TBot : class, IBot
        {
            services.AddTransient<IBot, TBot>();

            services.Configure(setupAction);

            return services;
        }
    }
}
