// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    /// <inheritdoc />
    public class BotFrameworkHttpClient : BotFrameworkHttpClientBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BotFrameworkHttpClient"/> class.
        /// </summary>
        /// <param name="httpClient">A <see cref="HttpClient"/>.</param>
        /// <param name="credentialProvider">An instance of <see cref="ICredentialProvider"/>.</param>
        /// <param name="channelProvider">An instance of <see cref="IChannelProvider"/>.</param>
        /// <param name="logger">An instance of <see cref="ILogger"/>.</param>
        public BotFrameworkHttpClient(
            HttpClient httpClient,
            ICredentialProvider credentialProvider,
            IChannelProvider channelProvider = null,
            ILogger logger = null)
            : base(httpClient, logger)
        {
            CredentialProvider = credentialProvider ?? throw new ArgumentNullException(nameof(credentialProvider));
            ChannelProvider = channelProvider;
        }

        /// <summary>
        /// Gets the channel provider for this adapter.
        /// </summary>
        /// <value>
        /// The channel provider for this adapter.
        /// </value>
        protected IChannelProvider ChannelProvider { get; }

        /// <summary>
        /// Gets the credential provider for this adapter.
        /// </summary>
        /// <value>
        /// The credential provider for this adapter.
        /// </value>
        protected ICredentialProvider CredentialProvider { get; }

        /// <inheritdoc />
        protected override async Task<AppCredentials> BuildCredentialsAsync(string appId, string oAuthScope = null)
        {
            var appPassword = await CredentialProvider.GetAppPasswordAsync(appId).ConfigureAwait(false);
            return ChannelProvider != null && ChannelProvider.IsGovernment() ? new MicrosoftGovernmentAppCredentials(appId, appPassword, HttpClient, Logger, oAuthScope) : new MicrosoftAppCredentials(appId, appPassword, HttpClient, Logger, oAuthScope);
        }

        /// <inheritdoc />
        protected override string GetOriginatingAudience()
        {
            return ChannelProvider != null && ChannelProvider.IsGovernment()
                ? GovernmentAuthenticationConstants.ToChannelFromBotOAuthScope
                : AuthenticationConstants.ToChannelFromBotOAuthScope;
        }
    }
}
