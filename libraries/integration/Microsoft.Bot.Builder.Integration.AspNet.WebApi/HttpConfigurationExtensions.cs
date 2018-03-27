// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Integration.AspNet.WebApi.Handlers;
using System;
using System.Web.Http;

namespace Microsoft.Bot.Builder.Integration.AspNet.WebApi
{
    public static class HttpConfigurationExtensions
    {
        public static HttpConfiguration MapBotFramework(this HttpConfiguration httpConfiguration, Action<BotFrameworkConfigurationBuilder> configurer)
        {
            var options = new BotFrameworkOptions();
            var optionsBuilder = new BotFrameworkConfigurationBuilder(options);

            configurer(optionsBuilder);

            ConfigureBotRoutes(BuildAdapter());

            return httpConfiguration;

            BotFrameworkAdapter BuildAdapter()
            {
                var adapter = new BotFrameworkAdapter(options.CredentialProvider, options.ConnectorClientRetryPolicy);

                foreach (var middleware in options.Middleware)
                {
                    adapter.Use(middleware);
                }

                return adapter;
            }

            void ConfigureBotRoutes(BotFrameworkAdapter adapter)
            {
                var routes = httpConfiguration.Routes;
                var baseUrl = options.Paths.BasePath;

                if (!baseUrl.EndsWith("/"))
                {
                    baseUrl += "/";
                }

                if (options.EnableProactiveMessages)
                {
                    routes.MapHttpRoute(
                        "BotFramework - Proactive Message Handler",
                        baseUrl + options.Paths.ProactiveMessagesPath,
                        defaults: null,
                        constraints: null,
                        handler: new BotProactiveMessageHandler(adapter));
                }

                routes.MapHttpRoute(
                        "BotFramework - Message Handler",
                        baseUrl + options.Paths.MessagesPath,
                        defaults: null,
                        constraints: null,
                        handler: new BotMessageHandler(adapter));
            }
        }        
    }
}