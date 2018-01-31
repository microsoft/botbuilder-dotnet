// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Bot.Connector
{
    public static class BotAuthenticationExtensions
    {
        public static AuthenticationBuilder AddBotAuthentication(this AuthenticationBuilder builder, string microsoftAppId, string microsoftAppPassword)
            => builder.AddBotAuthentication(new StaticCredentialProvider(microsoftAppId, microsoftAppPassword));

        public static AuthenticationBuilder AddBotAuthentication(this AuthenticationBuilder builder, ICredentialProvider credentialProvider)
        {
            builder.Services.AddSingleton(typeof(ICredentialProvider), credentialProvider);

            builder.AddBotAuthentication(JwtBearerDefaults.AuthenticationScheme, displayName: "botAuthenticator", configureOptions: options =>
            {
                options.CredentialProvider = credentialProvider;
                options.Events = new JwtBearerEvents();
            });

            return builder;
        }

        public static AuthenticationBuilder AddBotAuthentication(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<BotAuthenticationOptions> configureOptions)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<BotAuthenticationOptions>, JwtBearerPostConfigureOptions>());

            Action<BotAuthenticationOptions> wrappedOptions = options =>
            {
                configureOptions(options);
                builder.Services.AddSingleton(typeof(ICredentialProvider), options.CredentialProvider);
            };

            return builder.AddScheme<BotAuthenticationOptions, BotAuthenticationHandler>(authenticationScheme, displayName, wrappedOptions);
        }
    }
}