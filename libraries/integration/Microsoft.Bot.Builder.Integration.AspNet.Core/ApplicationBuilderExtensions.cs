// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Handlers;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    /// <summary>
    /// Extension methods for <see cref="IApplicationBuilder"/> to add a Bot to the ASP.NET Core request execution pipeline.
    /// </summary>
    /// <seealso cref="BotFrameworkPaths"/>
    /// <seealso cref="BotFrameworkAdapter"/>
    /// <seealso cref="ServiceCollectionExtensions"/>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Maps various endpoint handlers for the <see cref="ServiceCollectionExtensions.AddBot{TBot}(IServiceCollection, Action{BotFrameworkOptions})">registered bot</see> into the request execution pipeline.
        /// </summary>
        /// <param name="applicationBuilder">The <see cref="IApplicationBuilder"/>.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder UseBotFramework(this IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
            {
                throw new ArgumentNullException(nameof(applicationBuilder));
            }

            var applicationServices = applicationBuilder.ApplicationServices;

            var configuration = applicationServices.GetService<IConfiguration>();

            if (configuration != null)
            {
                var openIdEndpoint = configuration.GetSection(AuthenticationConstants.BotOpenIdMetadataKey)?.Value;

                if (!string.IsNullOrEmpty(openIdEndpoint))
                {
                    ChannelValidation.OpenIdMetadataUrl = openIdEndpoint;
                    GovernmentChannelValidation.OpenIdMetadataUrl = openIdEndpoint;
                }

                var oauthApiEndpoint = configuration.GetSection(AuthenticationConstants.OAuthUrlKey)?.Value;

                if (!string.IsNullOrEmpty(oauthApiEndpoint))
                {
                    OAuthClientConfig.OAuthEndpoint = oauthApiEndpoint;
                }

                var emulateOAuthCards = configuration.GetSection(AuthenticationConstants.EmulateOAuthCardsKey)?.Value;

                if (!string.IsNullOrEmpty(emulateOAuthCards) && bool.TryParse(emulateOAuthCards, out bool emulateOAuthCardsValue))
                {
                    OAuthClientConfig.EmulateOAuthCards = emulateOAuthCardsValue;
                }
            }

            var options = applicationServices.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;

            var paths = options.Paths;

            applicationBuilder.Map(
                paths.BasePath + paths.MessagesPath,
                botActivitiesAppBuilder => botActivitiesAppBuilder.Run(new BotMessageHandler().HandleAsync));

            return applicationBuilder;
        }

        /// <summary>
        /// Enables named pipes for this application.
        /// </summary>
        /// <param name="applicationBuilder">The application builder that defines the bot's pipeline.<see cref="IApplicationBuilder"/>.</param>
        /// <param name="pipeName">The name of the named pipe to use when creating the server.</param>
        /// <param name="audience">The specified recipient of all outgoing activities.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder UseNamedPipes(this IApplicationBuilder applicationBuilder, string pipeName = "bfv4.pipes", string audience = null)
        {
            if (applicationBuilder == null)
            {
                throw new ArgumentNullException(nameof(applicationBuilder));
            }

            return applicationBuilder.UseNamedPipes(pipeName, audience, null, null);
        }

        /// <summary>
        /// Enables named pipes for this application.
        /// </summary>
        /// <param name="applicationBuilder">The application builder that defines the bot's pipeline.<see cref="IApplicationBuilder"/>.</param>
        /// <param name="pipeName">The name of the named pipe to use when creating the server.</param>
        /// <param name="audience">The specified recipient of all outgoing activities.</param>
        /// <param name="appId">The bot's application id.</param>
        /// <param name="callerId">The caller id.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder UseNamedPipes(this IApplicationBuilder applicationBuilder, string pipeName, string audience, string appId, string callerId)
        {
            if (applicationBuilder == null)
            {
                throw new ArgumentNullException(nameof(applicationBuilder));
            }

            var bot = applicationBuilder.ApplicationServices.GetRequiredService<IBot>();

            var adapter = applicationBuilder.ApplicationServices.GetRequiredService<IBotFrameworkHttpAdapter>();

            if (typeof(BotFrameworkHttpAdapter).IsAssignableFrom(adapter.GetType()))
            {
                // back compatibility: support for the BotFrameworkHttpAdapter
                _ = ((BotFrameworkHttpAdapter)adapter).ConnectNamedPipeAsync(pipeName, bot, audience);
            }
            else if (typeof(CloudAdapter).IsAssignableFrom(adapter.GetType()))
            {
                // if any of these values are null then attempt to pull them from configuration
                var configuration = applicationBuilder.ApplicationServices.GetService<IConfiguration>();
                if (configuration != null)
                {
                    audience = audience ?? configuration.GetSection("ToChannelFromBotOAuthScope")?.Value ?? GetBuiltinDefaultAudience(configuration);
                    appId = appId ?? configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value;
                    callerId = callerId ?? configuration.GetSection("CallerId")?.Value;
                }

                _ = ((CloudAdapter)adapter).ConnectNamedPipeAsync(pipeName, bot, appId, audience, callerId);
            }

            return applicationBuilder;
        }

        private static string GetBuiltinDefaultAudience(IConfiguration configuration)
        {
            bool isPublicCloud = string.IsNullOrEmpty(configuration.GetSection("ChannelService")?.Value);
            return isPublicCloud ? AuthenticationConstants.ToChannelFromBotOAuthScope : GovernmentAuthenticationConstants.ToChannelFromBotOAuthScope;
        }
    }
}
