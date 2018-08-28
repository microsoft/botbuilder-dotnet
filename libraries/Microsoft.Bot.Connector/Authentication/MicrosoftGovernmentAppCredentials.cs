// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// MicrosoftGovernmentAppCredentials auth implementation
    /// </summary>
    public class MicrosoftGovernmentAppCredentials : MicrosoftAppCredentials
    {
        /// <summary>
        /// An empty set of credentials.
        /// </summary>
        public static new readonly MicrosoftGovernmentAppCredentials Empty = new MicrosoftGovernmentAppCredentials(null, null);
        
        static MicrosoftGovernmentAppCredentials()
        {
            TrustServiceUrl("https://api.botframework.us", DateTime.MaxValue);
            TrustServiceUrl("https://token.botframework.us", DateTime.MaxValue);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="MicrosoftGovernmentAppCredentials"/> class.
        /// </summary>
        /// <param name="appId">The Microsoft app ID.</param>
        /// <param name="password">The Microsoft app password.</param>
        public MicrosoftGovernmentAppCredentials(string appId, string password) : base(appId, password)
        {
        }

        /// <summary>
        /// Gets the OAuth endpoint to use.
        /// </summary>
        public override string OAuthEndpoint { get { return AuthenticationConstants.ToGovernmentChannelFromBotLoginUrl; } }

        /// <summary>
        /// Gets the OAuth scope to use.
        /// </summary>
        public override string OAuthScope { get { return AuthenticationConstants.ToGovernmentChannelFromBotOAuthScope; } }
    }
}
