// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Adapters;
using System;
using System.Web.Http;

namespace Microsoft.Bot.Builder.Integration.AspNet.WebApi
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
                var botActivitiesRouteUrl = options.RouteBaseUrl;

                if (!botActivitiesRouteUrl.EndsWith("/"))
                {
                    botActivitiesRouteUrl += "/";
                }

                botActivitiesRouteUrl += "activities";

                httpConfiguration.Routes.MapHttpRoute(
                        "BotFrameworkV4 Activities Controller",
                        botActivitiesRouteUrl,
                        defaults: null,
                        constraints: null,
                        handler: new BotActivitiesHandler(adapter));
            }
        }        
    }
}