// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    /// <summary>
    /// Extension methods for <see cref="IApplicationBuilder"/> to add a Bot to the ASP.NET Core request execution pipeline.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
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

            if (adapter is CloudAdapter cloudAdapter)
            {
                // if any of these values are null then attempt to pull them from configuration
                var configuration = applicationBuilder.ApplicationServices.GetService<IConfiguration>();
                if (configuration != null)
                {
                    audience ??= configuration.GetSection("ToChannelFromBotOAuthScope")?.Value ?? GetBuiltinDefaultAudience(configuration);
                    appId ??= configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value;
                    callerId ??= configuration.GetSection("CallerId")?.Value;
                }

                _ = cloudAdapter.ConnectNamedPipeAsync(pipeName, bot, appId, audience, callerId);
            }

            return applicationBuilder;
        }

        private static string GetBuiltinDefaultAudience(IConfiguration configuration)
        {
            var isPublicCloud = string.IsNullOrEmpty(configuration.GetSection("ChannelService")?.Value);
            return isPublicCloud ? AuthenticationConstants.ToChannelFromBotOAuthScope : GovernmentAuthenticationConstants.ToChannelFromBotOAuthScope;
        }
    }
}
