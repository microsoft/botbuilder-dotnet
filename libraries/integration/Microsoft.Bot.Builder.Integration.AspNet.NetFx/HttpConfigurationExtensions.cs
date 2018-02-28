// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Microsoft.Bot.Builder.Adapters;
using System;
using System.Web.Http;

namespace Microsoft.Bot.Builder.Integration.AspNet
{
    public static class HttpConfigurationExtensions
    {
        public static HttpConfiguration MapBotFramework(this HttpConfiguration httpConfiguration, Action<BotFrameworkConfigurationBuilder> configurer)
        {
            var optionsBuilder = new BotFrameworkConfigurationBuilder();

            configurer(optionsBuilder);

            var options = optionsBuilder.BotFrameworkOptions;

            ConfigureBotRoute(BuildAdapter());

            return httpConfiguration;

            BotFrameworkAdapter BuildAdapter()
            {
                var adapter = new BotFrameworkAdapter(options.AppId, options.AppPassword);

                foreach (var middleware in options.Middleware)
                {
                    adapter.Use(middleware);
                }

                return adapter;
            }

            void ConfigureBotRoute(BotFrameworkAdapter adapter)
            {
                var botMessagesRouteUrl = options.RouteBaseUrl;

                if (!botMessagesRouteUrl.EndsWith("/"))
                {
                    botMessagesRouteUrl += "/";
                }

                botMessagesRouteUrl += "activities";

                httpConfiguration.Routes.MapHttpRoute(
                        "BotFrameworkV4 Activities Controller",
                        botMessagesRouteUrl,
                        defaults: null,
                        constraints: null,
                        handler: new BotActivitiesHandler(adapter));
            }
        }        
    }
}