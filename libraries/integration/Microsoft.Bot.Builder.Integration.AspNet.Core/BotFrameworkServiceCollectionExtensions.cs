// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.StreamingExtensions
{
    /// <summary>
    /// Used to register adapter select service.
    /// </summary>
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
