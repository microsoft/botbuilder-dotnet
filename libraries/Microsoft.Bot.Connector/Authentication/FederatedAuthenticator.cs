// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.Rest;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Abstraction to acquire tokens from a Federated Credentials Application.
    /// </summary>
    internal class FederatedAuthenticator : IAuthenticator
    {
        private readonly string _authority;
        private readonly string _scope;
        private readonly string _clientId;
        private readonly ILogger _logger;
        private readonly IConfidentialClientApplication _clientApplication;
        private readonly ManagedIdentityClientAssertion _managedIdentityClientAssertion;
        private readonly IAuthorizationHeaderProvider _tokenProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="FederatedAuthenticator"/> class.
        /// </summary>
        /// <param name="tokenProvider">The authorization header provider to use for acquiring tokens.</param>
        /// <param name="appId">App id for the Application.</param>
        /// <param name="clientId">Client id for the managed identity to be used for acquiring tokens.</param>
        /// <param name="scope">Resource for which to acquire the token.</param>
        /// <param name="authority">Login endpoint for request.</param>
        /// <param name="customHttpClient">A customized instance of the HttpClient class.</param>
        /// <param name="logger">The type used to perform logging.</param>
        public FederatedAuthenticator(IAuthorizationHeaderProvider tokenProvider, string appId, string clientId, string authority, string scope, HttpClient customHttpClient = null, ILogger logger = null)
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }

            if (string.IsNullOrWhiteSpace(scope))
            {
                throw new ArgumentNullException(nameof(scope));
            }

            _tokenProvider = tokenProvider;
            _authority = authority;
            _scope = scope;
            _clientId = clientId;
            _logger = logger ?? NullLogger.Instance;
            _clientApplication = CreateClientApplication(appId, customHttpClient);
            _managedIdentityClientAssertion = new ManagedIdentityClientAssertion(_clientId);
        }

        /// <inheritdoc/>
        public async Task<AuthenticatorResult> GetTokenAsync(bool forceRefresh = false, string agentIdentity = "", string agentUser = "", string tenantId = "")
        {
            var watch = Stopwatch.StartNew();

            var result = await Retry
                .Run(() => AcquireTokenAsync(forceRefresh, agentIdentity, agentUser, tenantId), HandleTokenProviderException)
                .ConfigureAwait(false);

            watch.Stop();
            _logger.LogInformation($"GetTokenAsync: Acquired token using MSI in {watch.ElapsedMilliseconds}.");

            return result;
        }

        private async Task<AuthenticatorResult> AcquireTokenAsync(bool forceRefresh, string agentIdentity = "", string agentUser = "", string tenantId = "")
        {
            const string scopePostFix = "/.default";
            var scope = _scope;

            if (!scope.EndsWith(scopePostFix, StringComparison.OrdinalIgnoreCase))
            {
                scope = $"{scope}{scopePostFix}";
            }

            _logger.LogDebug($"AcquireTokenAsync: authority={_authority}, scope={scope}");

            if (!scope.Equals("https://api.botframework.com/.default", StringComparison.OrdinalIgnoreCase)
                            && string.IsNullOrEmpty(agentIdentity)
                            && string.IsNullOrEmpty(agentUser))
            {
                throw new InvalidOperationException("AcquireTokenAsync requires agentIdentity and agentUser to be set when using custom scopes.");
            }

            string token = string.Empty;
            if (string.IsNullOrEmpty(agentIdentity) && string.IsNullOrEmpty(agentUser))
            {
                // Acquire token async using MSAL.NET
                // This will use the cache from the application cache of the MSAL library, no external caching is needed.
                //msalResult = await _clientApplication
                //    .AcquireTokenForClient(new[] { scope })
                //    .WithAuthority(_authority ?? OAuthEndpoint, _validateAuthority)
                //    .WithForceRefresh(forceRefresh)
                //    .ExecuteAsync().ConfigureAwait(false);
                token = await _tokenProvider.CreateAuthorizationHeaderForAppAsync(scope);
            }
            else
            {
                token = await _tokenProvider.CreateAuthorizationHeaderAsync(
                    [scope],
                    new AuthorizationHeaderProviderOptions()
                    {
                        AcquireTokenOptions = new AcquireTokenOptions()
                        {
                            Tenant = tenantId
                        }
                    }.WithAgentUserIdentity(agentIdentity, Guid.Parse(agentUser)));
            }

            // This means we acquired a valid token successfully. We can make our retry policy null.
            return new AuthenticatorResult()
            {
                AccessToken = token.Substring("Bearer ".Length),
                ExpiresOn = DateTime.Now.AddMinutes(30)
            };
        }

        private RetryParams HandleTokenProviderException(Exception e, int retryCount)
        {
            _logger.LogError(e, "Exception when trying to acquire token using Federated Credentials!");

            if (e is MsalServiceException exception)
            {
                // stop retrying for all except for throttling response
                if (exception.StatusCode != 429)
                {
                    return RetryParams.StopRetrying;
                }
            }

            return RetryParams.DefaultBackOff(retryCount);
        }

        private IConfidentialClientApplication CreateClientApplication(string appId, HttpClient customHttpClient = null)
        {
            _logger.LogDebug($"CreateClientApplication for appId={appId}");

            var clientBuilder = ConfidentialClientApplicationBuilder
                .Create(appId)
                .WithClientAssertion((AssertionRequestOptions options) => FetchExternalTokenAsync())
                .WithCacheOptions(CacheOptions.EnableSharedCacheOptions); // for more cache options see https://learn.microsoft.com/entra/msal/dotnet/how-to/token-cache-serialization?tabs=msal

            if (customHttpClient != null)
            {
                clientBuilder.WithHttpClientFactory(new ConstantHttpClientFactory(customHttpClient));
            }

            return clientBuilder.Build();
        }

        private async Task<string> FetchExternalTokenAsync()
        {
            return await _managedIdentityClientAssertion.GetSignedAssertionAsync(default).ConfigureAwait(false);
        }
    }
}
