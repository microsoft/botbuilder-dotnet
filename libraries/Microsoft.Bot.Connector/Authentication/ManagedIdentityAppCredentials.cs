﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Managed Service Identity auth implementation.
    /// </summary>
    public class ManagedIdentityAppCredentials : AppCredentials
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedIdentityAppCredentials"/> class.
        /// Managed Identity for AAD credentials auth and caching.
        /// </summary>
        /// <param name="appId">Client ID for the managed identity assigned to the bot.</param>
        /// <param name="oAuthScope">The scope for the token.</param>
        /// <param name="customHttpClient">Optional <see cref="HttpClient"/> to be used when acquiring tokens.</param>
        /// <param name="logger">Optional <see cref="ILogger"/> to gather telemetry data while acquiring and managing credentials.</param>
        public ManagedIdentityAppCredentials(string appId, string oAuthScope, HttpClient customHttpClient = null, ILogger logger = null)
            : base(channelAuthTenant: null, customHttpClient, logger, oAuthScope)
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }

            MicrosoftAppId = appId;
        }

        /// <inheritdoc/>
        protected override Lazy<IAuthenticator> BuildIAuthenticator()
        {
            return new Lazy<IAuthenticator>(
                () => new ManagedIdentityAuthenticator(MicrosoftAppId, OAuthScope, CustomHttpClient, Logger),
                LazyThreadSafetyMode.ExecutionAndPublication);
        }
    }
}
