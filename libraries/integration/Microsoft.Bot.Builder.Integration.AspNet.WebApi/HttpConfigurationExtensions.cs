// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Integration.AspNet.WebApi.Handlers;
using Microsoft.Bot.Connector.Authentication;
using System;
using System.Configuration;
using System.Web.Http;

namespace Microsoft.Bot.Builder.Integration.AspNet.WebApi
{
    public static class HttpConfigurationExtensions
    {
        /// <summary>
        /// Map the Bot Framework into the request execution pipeline.
        /// </summary>
        /// <param name="httpConfiguration">The <see cref="HttpConfiguration" /> to map the bot into.</param>
        /// <param name="configurer">A callback to configure the bot.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static HttpConfiguration MapBotFramework(this HttpConfiguration httpConfiguration, Action<BotFrameworkConfigurationBuilder> configurer = null)
        {
            if (httpConfiguration == null)
            {
                throw new ArgumentNullException(nameof(httpConfiguration));
            }

            var options = new BotFrameworkOptions();
            var optionsBuilder = new BotFrameworkConfigurationBuilder(options);

            configurer?.Invoke(optionsBuilder);

            var botFrameworkAdapter = GetOrCreateBotFrameworkAdapter();

            ConfigureMiddleware(botFrameworkAdapter);
            ConfigureBotRoutes(botFrameworkAdapter);

            return httpConfiguration;

            BotFrameworkAdapter GetOrCreateBotFrameworkAdapter()
            {
                if (!(httpConfiguration.DependencyResolver.GetService(typeof(BotFrameworkAdapter)) is BotFrameworkAdapter adapter))
                {
                    var credentialProvider = ResolveCredentialProvider();

                    adapter = new BotFrameworkAdapter(credentialProvider, options.ConnectorClientRetryPolicy, options.HttpClient);
                }

                return adapter;
            }

            void ConfigureMiddleware(BotFrameworkAdapter adapter)
            {
                foreach (var middleware in options.Middleware)
                {
                    adapter.Use(middleware);
                }
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
                        BotProactiveMessageHandler.RouteName,
                        baseUrl + options.Paths.ProactiveMessagesPath,
                        defaults: null,
                        constraints: null,
                        handler: new BotProactiveMessageHandler(adapter));
                }

                routes.MapHttpRoute(
                        BotMessageHandler.RouteName,
                        baseUrl + options.Paths.MessagesPath,
                        defaults: null,
                        constraints: null,
                        handler: new BotMessageHandler(adapter));
            }

            ICredentialProvider ResolveCredentialProvider()
            {
                var credentialProvider = options.CredentialProvider;

                // If a credential provider was explicitly configured, just return that straight away
                if (credentialProvider != null)
                {
                    return credentialProvider;
                }

                return new SimpleCredentialProvider(ConfigurationManager.AppSettings[MicrosoftAppCredentials.MicrosoftAppIdKey], ConfigurationManager.AppSettings[MicrosoftAppCredentials.MicrosoftAppPasswordKey]);
            }
        }
    }
}