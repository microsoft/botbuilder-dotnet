// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.AppConfig;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Abstraction to acquire tokens from a Managed Service Identity.
    /// </summary>
    public class ManagedIdentityAuthenticator : IAuthenticator
    {
        private readonly string _resource;
        private readonly ILogger _logger;
        private readonly IManagedIdentityApplication _clientApplication;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedIdentityAuthenticator"/> class.
        /// </summary>
        /// <param name="appId">Client id for the managed identity to be used for acquiring tokens.</param>
        /// <param name="resource">Resource for which to acquire the token.</param>
        /// <param name="customHttpClient">A customized instance of the HttpClient class.</param>
        /// <param name="logger">The type used to perform logging.</param>
        public ManagedIdentityAuthenticator(string appId, string resource, HttpClient customHttpClient = null, ILogger logger = null)
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }

            if (string.IsNullOrWhiteSpace(resource))
            {
                throw new ArgumentNullException(nameof(resource));
            }
            
            _resource = resource;
            _logger = logger ?? NullLogger.Instance;
            _clientApplication = CreateClientApplication(appId, customHttpClient);
        }

        /// <inheritdoc/>
        public async Task<AuthenticatorResult> GetTokenAsync(bool forceRefresh = false)
        {
            var watch = Stopwatch.StartNew();

            var result = await Retry
                .Run(() => AcquireTokenAsync(forceRefresh), HandleTokenProviderException)
                .ConfigureAwait(false);

            watch.Stop();
            _logger.LogInformation($"GetTokenAsync: Acquired token using MSI in {watch.ElapsedMilliseconds}.");

            return result;
        }

        private async Task<AuthenticatorResult> AcquireTokenAsync(bool forceRefresh)
        {
            var authResult = await _clientApplication
                .AcquireTokenForManagedIdentity(_resource)
                .WithForceRefresh(forceRefresh)
                .ExecuteAsync()
                .ConfigureAwait(false);
            return new AuthenticatorResult
            {
                AccessToken = authResult.AccessToken,
                ExpiresOn = authResult.ExpiresOn
            };
        }

        private RetryParams HandleTokenProviderException(Exception e, int retryCount)
        {
            _logger.LogError(e, "Exception when trying to acquire token using MSI!");

            return e is MsalServiceException // BadRequest
                ? RetryParams.StopRetrying
                : RetryParams.DefaultBackOff(retryCount);
        }

        private IManagedIdentityApplication CreateClientApplication(string appId, HttpClient customHttpClient = null)
        {
            var clientBuilder = ManagedIdentityApplicationBuilder.Create(ManagedIdentityId.WithUserAssignedClientId(appId));

            if (customHttpClient != null)
            {
                clientBuilder.WithHttpClientFactory(new ConstantHttpClientFactory(customHttpClient));
            }

            return clientBuilder.Build();
        }
    }
}
