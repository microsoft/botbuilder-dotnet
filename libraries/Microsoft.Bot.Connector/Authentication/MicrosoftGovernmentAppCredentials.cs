// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
        public static new readonly MicrosoftGovernmentAppCredentials Empty = new MicrosoftGovernmentAppCredentials(null, null);

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftGovernmentAppCredentials"/> class.
        /// </summary>
        /// <param name="appId">The Microsoft app ID.</param>
        /// <param name="password">The Microsoft app password.</param>
        /// <param name="customHttpClient">Optional <see cref="HttpClient"/> to be used when acquiring tokens.</param>
        public MicrosoftGovernmentAppCredentials(string appId, string password, HttpClient customHttpClient = null)
            : base(appId, password, customHttpClient)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftGovernmentAppCredentials"/> class.
        /// </summary>
        /// <param name="appId">The Microsoft app ID.</param>
        /// <param name="password">The Microsoft app password.</param>
        /// <param name="customHttpClient">Optional <see cref="HttpClient"/> to be used when acquiring tokens.</param>
        /// <param name="logger">Optional <see cref="ILogger"/> to gather telemetry data while acquiring and managing credentials.</param>
        public MicrosoftGovernmentAppCredentials(string appId, string password, HttpClient customHttpClient, ILogger logger)
            : base(appId, password, customHttpClient, logger)
        {
        }

        /// <summary>
        /// Gets the OAuth endpoint to use.
        /// </summary>
        /// <value>
        /// The OAuth endpoint to use.
        /// </value>
        public override string OAuthEndpoint
        {
            get { return GovernmentAuthenticationConstants.ToChannelFromBotLoginUrl; }
        }

        /// <summary>
        /// Gets the OAuth scope to use.
        /// </summary>
        /// <value>
        /// The OAuth scope to use.
        /// </value>
        public override string OAuthScope
        {
            get { return GovernmentAuthenticationConstants.ToChannelFromBotOAuthScope; }
        }
    }
}
