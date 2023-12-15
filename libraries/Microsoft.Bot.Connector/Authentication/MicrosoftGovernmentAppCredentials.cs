// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Globalization;
using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// MicrosoftGovernmentAppCredentials auth implementation.
    /// </summary>
    public class MicrosoftGovernmentAppCredentials : MicrosoftAppCredentials
    {
        /// <summary>
        /// An empty set of credentials.
        /// </summary>
        public static new readonly MicrosoftGovernmentAppCredentials Empty = new MicrosoftGovernmentAppCredentials(null, null, null, null, GovernmentAuthenticationConstants.ToChannelFromBotOAuthScope);

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftGovernmentAppCredentials"/> class.
        /// </summary>
        /// <param name="appId">The Microsoft app ID.</param>
        /// <param name="password">The Microsoft app password.</param>
        /// <param name="customHttpClient">Optional <see cref="HttpClient"/> to be used when acquiring tokens.</param>
        /// <param name="logger">Optional <see cref="ILogger"/> to gather telemetry data while acquiring and managing credentials.</param>
        /// <param name="oAuthScope">The scope for the token (defaults to <see cref="GovernmentAuthenticationConstants.ToChannelFromBotOAuthScope"/> if null).</param>
        public MicrosoftGovernmentAppCredentials(string appId, string password, HttpClient customHttpClient = null, ILogger logger = null, string oAuthScope = null)
            : base(appId, password, customHttpClient, logger, oAuthScope)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftGovernmentAppCredentials"/> class.
        /// </summary>
        /// <param name="appId">The Microsoft app ID.</param>
        /// <param name="password">The Microsoft app password.</param>
        /// <param name="channelAuthTenant">Optional. The oauth token tenant.</param>
        /// <param name="customHttpClient">Optional <see cref="HttpClient"/> to be used when acquiring tokens.</param>
        /// <param name="logger">Optional <see cref="ILogger"/> to gather telemetry data while acquiring and managing credentials.</param>
        /// <param name="oAuthScope">The scope for the token (defaults to <see cref="GovernmentAuthenticationConstants.ToChannelFromBotOAuthScope"/> if null).</param>
        public MicrosoftGovernmentAppCredentials(string appId, string password, string channelAuthTenant, HttpClient customHttpClient = null, ILogger logger = null, string oAuthScope = null)
            : base(appId, password, channelAuthTenant, customHttpClient, logger, oAuthScope)
        {
        }

        /// <inheritdoc/>
        protected override string DefaultChannelAuthTenant => GovernmentAuthenticationConstants.DefaultChannelAuthTenant;

        /// <inheritdoc/>
        protected override string ToChannelFromBotOAuthScope => GovernmentAuthenticationConstants.ToChannelFromBotOAuthScope;

        /// <inheritdoc/>
        protected override string ToChannelFromBotLoginUrlTemplate => GovernmentAuthenticationConstants.ToChannelFromBotLoginUrlTemplate;
    }
}
