// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Runtime.Builders.Middleware;
using Microsoft.Bot.Runtime.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Microsoft.Bot.Runtime.Providers.Adapter
{
    [JsonObject]
    public class BotCoreAdapterProvider : IAdapterProvider
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.BotCoreAdapter";

        [JsonProperty("middleware")]
        public IList<IMiddlewareBuilder> Middleware { get; } = new List<IMiddlewareBuilder>();

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            services.Configure<CoreBotAdapterOptions>(o =>
            {
                foreach (IMiddlewareBuilder middleware in this.Middleware)
                {
                    o.Middleware.Add(middleware);
                }
            });

            services.AddSingleton<IBotFrameworkHttpAdapter, CoreBotAdapter>();
            services.AddSingleton<BotAdapter>(
                sp => (BotFrameworkHttpAdapter)sp.GetService<IBotFrameworkHttpAdapter>());
        }
    }
}
