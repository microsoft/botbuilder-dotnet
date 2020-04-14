// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

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
        public MicrosoftGovernmentAppCredentials(string appId, string password, HttpClient customHttpClient = null)
            : this(appId, password, customHttpClient, null, GovernmentAuthenticationConstants.ToChannelFromBotOAuthScope)
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
            : this(appId, password, customHttpClient, logger, GovernmentAuthenticationConstants.ToChannelFromBotOAuthScope)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftGovernmentAppCredentials"/> class.
        /// </summary>
        /// <param name="appId">The Microsoft app ID.</param>
        /// <param name="password">The Microsoft app password.</param>
        /// <param name="customHttpClient">Optional <see cref="HttpClient"/> to be used when acquiring tokens.</param>
        /// <param name="logger">Optional <see cref="ILogger"/> to gather telemetry data while acquiring and managing credentials.</param>
        /// <param name="oAuthScope">The scope for the token (defaults to <see cref="GovernmentAuthenticationConstants.ToChannelFromBotOAuthScope"/> if null).</param>
        public MicrosoftGovernmentAppCredentials(string appId, string password, HttpClient customHttpClient, ILogger logger, string oAuthScope = null)
            : base(appId, password, customHttpClient, logger, oAuthScope ?? GovernmentAuthenticationConstants.ToChannelFromBotOAuthScope)
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

        protected override Lazy<IAuthenticator> BuildIAuthenticator()
        {
            return new Lazy<IAuthenticator>(
                () =>
                {
                    if (OAuthScope == GovernmentAuthenticationConstants.ToChannelFromBotOAuthScope)
                    {
                        var optionV1 = AuthenticatorFor(GovernmentAuthenticationConstants.ToChannelFromBotLoginUrl, GovernmentAuthenticationConstants.ToChannelFromBotOAuthScope);
                        var optionV2 = AuthenticatorFor(GovernmentAuthenticationConstants.ToChannelFromBotLoginUrlV2, GovernmentAuthenticationConstants.ToChannelFromBotOAuthScopeV2);

                        return new Authenticators(optionV2, optionV1);
                    }
                    else
                    {
                        return AuthenticatorFor(OAuthEndpoint, OAuthScope);
                    }
                },
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        private IAuthenticator AuthenticatorFor(string authority, string scope)
        {
            var credential = new ClientCredential(MicrosoftAppId, MicrosoftAppPassword);
            var configuration = new OAuthConfiguration() { Authority = authority, Scope = scope };
            return new AdalAuthenticator(credential, configuration, this.CustomHttpClient, this.Logger);
        }
    }
}
