// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Newtonsoft.Json;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Base abstraction for AAD credentials for auth and caching.
    /// </summary>
    public abstract class AppCredentials : ServiceClientCredentials
    {
        private static readonly IDictionary<string, DateTime> TrustedHostNames = new Dictionary<string, DateTime>()
        {
            // { "state.botframework.com", DateTime.MaxValue }, // deprecated state api
            { "api.botframework.com", DateTime.MaxValue },       // bot connector API
            { "token.botframework.com", DateTime.MaxValue },    // oauth token endpoint
            { "api.botframework.azure.us", DateTime.MaxValue },        // bot connector API in US Government DataCenters
            { "token.botframework.azure.us", DateTime.MaxValue },      // oauth token endpoint in US Government DataCenters
        };

        /// <summary>
        /// Authenticator abstraction used to obtain tokens through the Client Credentials OAuth 2.0 flow.
        /// </summary>
        private readonly Lazy<AdalAuthenticator> authenticator;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppCredentials"/> class.
        /// </summary>
        /// <param name="channelAuthTenant">Optional. The oauth token tenant.</param>
        /// <param name="customHttpClient">Optional <see cref="HttpClient"/> to be used when acquiring tokens.</param>
        /// <param name="logger">Optional <see cref="ILogger"/> to gather telemetry data while acquiring and managing credentials.</param>
        public AppCredentials(string channelAuthTenant = null, HttpClient customHttpClient = null, ILogger logger = null)
        {
            authenticator = BuildAuthenticator();
            this.ChannelAuthTenant = channelAuthTenant;
            this.CustomHttpClient = customHttpClient;
            this.Logger = logger;
        }

        /// <summary>
        /// Gets or sets the Microsoft app ID for this credential.
        /// </summary>
        /// <value>
        /// The Microsoft app ID for this credential.
        /// </value>
        public string MicrosoftAppId { get; set; }

        /// <summary>
        /// Gets or sets tenant to be used for channel authentication.
        /// </summary>
        /// <value>
        /// Tenant to be used for channel authentication.
        /// </value>
        public string ChannelAuthTenant
        {
            get
            {
                return string.IsNullOrEmpty(AuthTenant)
                    ? AuthenticationConstants.DefaultChannelAuthTenant
                    : AuthTenant;
            }

            set
            {
                // Advanced user only, see https://aka.ms/bots/tenant-restriction
                var endpointUrl = string.Format(AuthenticationConstants.ToChannelFromBotLoginUrlTemplate, value);

                if (Uri.TryCreate(endpointUrl, UriKind.Absolute, out var result))
                {
                    AuthTenant = value;
                }
                else
                {
                    throw new Exception($"Invalid channel auth tenant: {value}");
                }
            }
        }

        /// <summary>
        /// Gets the OAuth endpoint to use.
        /// </summary>
        /// <value>
        /// The OAuth endpoint to use.
        /// </value>
        public virtual string OAuthEndpoint => string.Format(AuthenticationConstants.ToChannelFromBotLoginUrlTemplate, ChannelAuthTenant);

        /// <summary>
        /// Gets the OAuth scope to use.
        /// </summary>
        /// <value>
        /// The OAuth scope to use.
        /// </value>
        public virtual string OAuthScope => AuthenticationConstants.ToChannelFromBotOAuthScope;

        /// <summary>
        /// Gets or sets the channel auth token tenant for this credential.
        /// </summary>
        /// <value>
        /// The channel auth token tenant for this credential.
        /// </value>
        protected string AuthTenant { get; set; }

        /// <summary>
        /// Gets or sets the channel auth token tenant for this credential.
        /// </summary>
        /// <value>
        /// The channel auth token tenant for this credential.
        /// </value>
        protected HttpClient CustomHttpClient { get; set; }

        /// <summary>
        /// Gets or sets the channel auth token tenant for this credential.
        /// </summary>
        /// <value>
        /// The channel auth token tenant for this credential.
        /// </value>
        protected ILogger Logger { get; set; }

        /// <summary>
        /// Adds the host of service url to <see cref="MicrosoftAppCredentials"/> trusted hosts.
        /// </summary>
        /// <param name="serviceUrl">The service URL.</param>
        /// <remarks>If expiration time is not provided, the expiration time will DateTime.UtcNow.AddDays(1).</remarks>
        public static void TrustServiceUrl(string serviceUrl)
        {
            TrustServiceUrl(serviceUrl, DateTime.UtcNow.Add(TimeSpan.FromDays(1)));
        }

        /// <summary>
        /// Adds the host of service url to <see cref="MicrosoftAppCredentials"/> trusted hosts.
        /// </summary>
        /// <param name="serviceUrl">The service URL.</param>
        /// <param name="expirationTime">The expiration time after which this service url is not trusted anymore.</param>
        public static void TrustServiceUrl(string serviceUrl, DateTime expirationTime)
        {
            lock (TrustedHostNames)
            {
                TrustedHostNames[new Uri(serviceUrl).Host] = expirationTime;
            }
        }

        /// <summary>
        /// Checks if the service url is for a trusted host or not.
        /// </summary>
        /// <param name="serviceUrl">The service url.</param>
        /// <returns>True if the host of the service url is trusted; False otherwise.</returns>
        public static bool IsTrustedServiceUrl(string serviceUrl)
        {
            if (Uri.TryCreate(serviceUrl, UriKind.Absolute, out Uri uri))
            {
                return IsTrustedUrl(uri);
            }

            return false;
        }

        /// <summary>
        /// Apply the credentials to the HTTP request.
        /// </summary>
        /// <param name="request">The HTTP request.</param><param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (ShouldSetToken(request))
            {
                string token = await this.GetTokenAsync().ConfigureAwait(false);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            await base.ProcessHttpRequestAsync(request, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets an OAuth access token.
        /// </summary>
        /// <param name="forceRefresh">True to force a refresh of the token; or false to get
        /// a cached token if it exists.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful, the result contains the access token string.</remarks>
        public async Task<string> GetTokenAsync(bool forceRefresh = false)
        {
            var token = await authenticator.Value.GetTokenAsync(forceRefresh).ConfigureAwait(false);
            return token.AccessToken;
        }

        /// <summary>
        /// Builds the lazy <see cref="AdalAuthenticator" /> to be used for token acquisition.
        /// </summary>
        /// <returns>A lazy <see cref="AdalAuthenticator"/>.</returns>
        protected abstract Lazy<AdalAuthenticator> BuildAuthenticator();

        private static bool IsTrustedUrl(Uri uri)
        {
            lock (TrustedHostNames)
            {
                if (TrustedHostNames.TryGetValue(uri.Host, out DateTime trustedServiceUrlExpiration))
                {
                    // check if the trusted service url is still valid
                    if (trustedServiceUrlExpiration > DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(5)))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private bool ShouldSetToken(HttpRequestMessage request)
        {
            if (IsTrustedUrl(request.RequestUri))
            {
                return true;
            }

            System.Diagnostics.Debug.WriteLine($"Service url {request.RequestUri.Authority} is not trusted and JwtToken cannot be sent to it.");
            return false;
        }
    }
}
