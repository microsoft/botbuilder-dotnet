// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Rest;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Base abstraction for AAD credentials for auth and caching.
    /// </summary>
    public abstract class AppCredentials : ServiceClientCredentials
    {
        internal static readonly IDictionary<string, DateTime> TrustedHostNames = new Dictionary<string, DateTime>
        {
            // { "state.botframework.com", DateTime.MaxValue }, // deprecated state api
            { "api.botframework.com", DateTime.MaxValue }, // bot connector API
            { "token.botframework.com", DateTime.MaxValue }, // oauth token endpoint
            { "api.botframework.azure.us", DateTime.MaxValue }, // bot connector API in US Government DataCenters
            { "token.botframework.azure.us", DateTime.MaxValue }, // oauth token endpoint in US Government DataCenters
        };

        /// <summary>
        /// Authenticator abstraction used to obtain tokens through the Client Credentials OAuth 2.0 flow.
        /// </summary>
        private Lazy<IAuthenticator> _authenticator;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppCredentials"/> class.
        /// </summary>
        /// <param name="channelAuthTenant">Optional. The oauth token tenant.</param>
        /// <param name="customHttpClient">Optional <see cref="HttpClient"/> to be used when acquiring tokens.</param>
        /// <param name="logger">Optional <see cref="ILogger"/> to gather telemetry data while acquiring and managing credentials.</param>
        public AppCredentials(string channelAuthTenant = null, HttpClient customHttpClient = null, ILogger logger = null)
            : this(channelAuthTenant, customHttpClient, logger, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppCredentials"/> class.
        /// </summary>
        /// <param name="channelAuthTenant">Optional. The oauth token tenant.</param>
        /// <param name="customHttpClient">Optional <see cref="HttpClient"/> to be used when acquiring tokens.</param>
        /// <param name="logger">Optional <see cref="ILogger"/> to gather telemetry data while acquiring and managing credentials.</param>
        /// <param name="oAuthScope">The scope for the token.</param>
        public AppCredentials(string channelAuthTenant = null, HttpClient customHttpClient = null, ILogger logger = null, string oAuthScope = null)
        {
            OAuthScope = string.IsNullOrWhiteSpace(oAuthScope) ? AuthenticationConstants.ToChannelFromBotOAuthScope : oAuthScope;
            ChannelAuthTenant = channelAuthTenant;
            CustomHttpClient = customHttpClient;
            Logger = logger ?? NullLogger.Instance;
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
            get => string.IsNullOrEmpty(AuthTenant) ? AuthenticationConstants.DefaultChannelAuthTenant : AuthTenant;
            set
            {
                // Advanced user only, see https://aka.ms/bots/tenant-restriction
                var endpointUrl = string.Format(CultureInfo.InvariantCulture, AuthenticationConstants.ToChannelFromBotLoginUrlTemplate, value);

                if (Uri.TryCreate(endpointUrl, UriKind.Absolute, out _))
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
        public virtual string OAuthEndpoint => string.Format(CultureInfo.InvariantCulture, AuthenticationConstants.ToChannelFromBotLoginUrlTemplate, ChannelAuthTenant);

        /// <summary>
        /// Gets the OAuth scope to use.
        /// </summary>
        /// <value>
        /// The OAuth scope to use.
        /// </value>
        public virtual string OAuthScope { get; }

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
            if (Uri.TryCreate(serviceUrl, UriKind.Absolute, out var uri))
            {
                return IsTrustedUrl(uri, NullLogger.Instance);
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
            if (ShouldSetToken(request, Logger))
            {
                var token = await GetTokenAsync().ConfigureAwait(false);
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
            _authenticator ??= BuildIAuthenticator();
            var token = await _authenticator.Value.GetTokenAsync(forceRefresh).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(token.AccessToken))
            {
                Logger.LogWarning($"{GetType().FullName}.ProcessHttpRequestAsync(): got empty token from call to the configured IAuthenticator.");
            }

            return token.AccessToken;
        }

        /// <summary>
        /// Builds the lazy <see cref="AdalAuthenticator" /> to be used for token acquisition.
        /// </summary>
        /// <returns>A lazy <see cref="AdalAuthenticator"/>.</returns>
        protected abstract Lazy<AdalAuthenticator> BuildAuthenticator();

        /// <summary>
        /// Builds the lazy <see cref="IAuthenticator" /> to be used for token acquisition.
        /// </summary>
        /// <returns>A lazy <see cref="IAuthenticator"/>.</returns>
        protected virtual Lazy<IAuthenticator> BuildIAuthenticator()
        {
            return new Lazy<IAuthenticator>(
                () =>
                {
                    var lazyAuthenticator = BuildAuthenticator();
                    return lazyAuthenticator.Value;
                },
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        private static bool IsTrustedUrl(Uri uri, ILogger logger)
        {
            lock (TrustedHostNames)
            {
                if (TrustedHostNames.TryGetValue(uri.Host, out var trustedServiceUrlExpiration))
                {
                    // check if the trusted service url is still valid
                    if (trustedServiceUrlExpiration > DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(5)))
                    {
                        return true;
                    }

                    logger.LogWarning($"{typeof(AppCredentials).FullName}.IsTrustedUrl(): '{uri}' found in TrustedHostNames but it expired (Expiration is set to: {trustedServiceUrlExpiration}, current time is {DateTime.UtcNow}).");
                    return false;
                }

                logger.LogWarning($"{typeof(AppCredentials).FullName}.IsTrustedUrl(): '{uri}' not found in TrustedHostNames.");
                return false;
            }
        }

        private static bool ShouldSetToken(HttpRequestMessage request, ILogger logger)
        {
            if (IsTrustedUrl(request.RequestUri, logger))
            {
                return true;
            }

            logger.LogWarning($"{typeof(AppCredentials).FullName}.ShouldSetToken(): '{request.RequestUri.Authority}' is not trusted and JwtToken cannot be sent to it.");
            return false;
        }
    }
}
