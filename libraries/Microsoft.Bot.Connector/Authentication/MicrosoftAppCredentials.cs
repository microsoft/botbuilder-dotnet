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
    /// MicrosoftAppCredentials auth implementation and cache.
    /// </summary>
    public class MicrosoftAppCredentials : AppCredentials
    {
        /// <summary>
        /// The configuration property for the App type of the bot -- MultiTenant, SingleTenant, or, MSI.
        /// </summary>
        public const string MicrosoftAppTypeKey = "MicrosoftAppType";

        /// <summary>
        /// The configuration property for the Microsoft app Password.
        /// </summary>
        public const string MicrosoftAppPasswordKey = "MicrosoftAppPassword";

        /// <summary>
        /// The configuration property for the Microsoft app ID.
        /// </summary>
        public const string MicrosoftAppIdKey = "MicrosoftAppId";

        /// <summary>
        /// The configuration property for Tenant ID of the Azure AD tenant.
        /// </summary>
        public const string MicrosoftAppTenantIdKey = "MicrosoftAppTenantId";

        /// <summary>
        /// An empty set of credentials.
        /// </summary>
        public static readonly MicrosoftAppCredentials Empty = new MicrosoftAppCredentials(null, null);

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftAppCredentials"/> class.
        /// </summary>
        /// <param name="appId">The Microsoft app ID.</param>
        /// <param name="password">The Microsoft app password.</param>
        /// <param name="customHttpClient">Optional <see cref="HttpClient"/> to be used when acquiring tokens.</param>
        /// <param name="logger">Optional <see cref="ILogger"/> to gather telemetry data while acquiring and managing credentials.</param>
        /// <param name="oAuthScope">The scope for the token.</param>
        public MicrosoftAppCredentials(string appId, string password, HttpClient customHttpClient = null, ILogger logger = null, string oAuthScope = null)
            : this(appId, password, null, customHttpClient, logger, oAuthScope)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftAppCredentials"/> class.
        /// </summary>
        /// <param name="appId">The Microsoft app ID.</param>
        /// <param name="password">The Microsoft app password.</param>
        /// <param name="channelAuthTenant">Optional. The oauth token tenant.</param>
        /// <param name="customHttpClient">Optional <see cref="HttpClient"/> to be used when acquiring tokens.</param>
        /// <param name="logger">Optional <see cref="ILogger"/> to gather telemetry data while acquiring and managing credentials.</param>
        /// <param name="oAuthScope">The scope for the token.</param>
        public MicrosoftAppCredentials(string appId, string password, string channelAuthTenant, HttpClient customHttpClient = null, ILogger logger = null, string oAuthScope = null)
            : base(channelAuthTenant, customHttpClient, logger, oAuthScope)
        {
            MicrosoftAppId = appId;
            MicrosoftAppPassword = password;
        }

        /// <summary>
        /// Gets or sets the Microsoft app password for this credential.
        /// </summary>
        /// <value>
        /// The Microsoft app password for this credential.
        /// </value>
        public string MicrosoftAppPassword { get; set; }

        /// <summary>
        /// Builds the lazy <see cref="AdalAuthenticator" /> to be used for token acquisition.
        /// </summary>
        /// <returns>A lazy <see cref="AdalAuthenticator"/>.</returns>
        [Obsolete("This method is deprecated. Use BuildIAuthenticator instead.", false)]
        protected override Lazy<AdalAuthenticator> BuildAuthenticator()
        {
            return new Lazy<AdalAuthenticator>(
                () =>
                new AdalAuthenticator(
                    new ClientCredential(MicrosoftAppId, MicrosoftAppPassword),
                    new OAuthConfiguration() { Authority = OAuthEndpoint, ValidateAuthority = ValidateAuthority, Scope = OAuthScope },
                    this.CustomHttpClient,
                    this.Logger),
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <inheritdoc/>
        protected override Lazy<IAuthenticator> BuildIAuthenticator()
        {
            return new Lazy<IAuthenticator>(
                () =>
                {
                    var clientApplication = CreateClientApplication(MicrosoftAppId, MicrosoftAppPassword, CustomHttpClient);
                    return new MsalAppCredentials(
                        clientApplication,
                        MicrosoftAppId,
                        OAuthEndpoint,
                        OAuthScope,
                        ValidateAuthority,
                        Logger);
                },
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        private Identity.Client.IConfidentialClientApplication CreateClientApplication(string appId, string password, HttpClient customHttpClient = null)
        {
            var clientBuilder = Identity.Client.ConfidentialClientApplicationBuilder.Create(appId)
               .WithAuthority(new Uri(OAuthEndpoint), ValidateAuthority)
               .WithClientSecret(password);

            if (customHttpClient != null)
            {
                clientBuilder.WithHttpClientFactory(new ConstantHttpClientFactory(customHttpClient));
            }

            return clientBuilder.Build();
        }
    }
}
