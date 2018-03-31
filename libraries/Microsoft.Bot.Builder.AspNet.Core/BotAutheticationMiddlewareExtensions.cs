// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Bot.Builder.AspNet.Core
{
    /// <summary>
    /// Bot authentication middleware extensions.
    /// </summary>
    public static class BotAutheticationMiddlewareExtensions
    {
        /// <summary>
        /// Adds the bot authentication.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>Authentication builder.</returns>
        public static AuthenticationBuilder AddBotAuthentication(this AuthenticationBuilder builder)
        {
            builder.AddBotAuthentication(JwtBearerDefaults.AuthenticationScheme, displayName: "botAuthenticator", configureOptions: options =>
            {
                options.Events = new JwtBearerEvents();
            });

            return builder;
        }

        /// <summary>
        /// Adds the bot authentication.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="authenticationScheme">The authentication scheme.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="configureOptions">The configure options.</param>
        /// <returns>Authentication builder.</returns>
        public static AuthenticationBuilder AddBotAuthentication(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<BotAuthenticationOptions> configureOptions)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<BotAuthenticationOptions>, JwtBearerPostConfigureOptions>());

            Action<BotAuthenticationOptions> wrappedOptions = options =>
            {
                configureOptions(options);
            };

            return builder.AddScheme<BotAuthenticationOptions, BotAuthenticationMiddleware>(authenticationScheme, displayName, wrappedOptions);
        }
    }
}
