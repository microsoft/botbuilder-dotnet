// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Abstraction to acquire tokens from a Managed Service Identity.
    /// </summary>
    public class ManagedIdentityAuthenticator : IAuthenticator
    {
        private readonly AzureServiceTokenProvider _tokenProvider;
        private readonly string _resource;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedIdentityAuthenticator"/> class.
        /// </summary>
        /// <param name="appId">Client id for the managed identity to be used for acquiring tokens.</param>
        /// <param name="resource">Resource for which to acquire the token.</param>
        /// <param name="tokenProviderFactory">The JWT token provider factory to use.</param>
        /// <param name="customHttpClient">A customized instance of the HttpClient class.</param>
        /// <param name="logger">The type used to perform logging.</param>
        public ManagedIdentityAuthenticator(string appId, string resource, IJwtTokenProviderFactory tokenProviderFactory, HttpClient customHttpClient = null, ILogger logger = null)
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }

            if (string.IsNullOrWhiteSpace(resource))
            {
                throw new ArgumentNullException(nameof(resource));
            }

            if (tokenProviderFactory == null)
            {
                throw new ArgumentNullException(nameof(tokenProviderFactory));
            }

            _resource = resource;
            _tokenProvider = tokenProviderFactory.CreateAzureServiceTokenProvider(appId, customHttpClient);
            _logger = logger ?? NullLogger.Instance;
        }

        /// <inheritdoc/>
        public async Task<AuthenticatorResult> GetTokenAsync(bool forceRefresh = false)
        {
            var watch = Stopwatch.StartNew();

            var result = await Retry
                .RunAsync(() => AcquireTokenAsync(forceRefresh), HandleTokenProviderException)
                .ConfigureAwait(false);

            watch.Stop();
            Log.AcquiredToken(_logger, nameof(GetTokenAsync), watch.ElapsedMilliseconds);

            return result;
        }

        private async Task<AuthenticatorResult> AcquireTokenAsync(bool forceRefresh)
        {
            var authResult = await _tokenProvider.GetAuthenticationResultAsync(_resource, forceRefresh).ConfigureAwait(false);
            return new AuthenticatorResult
            {
                AccessToken = authResult.AccessToken,
                ExpiresOn = authResult.ExpiresOn
            };
        }

        private RetryParams HandleTokenProviderException(Exception e, int retryCount)
        {
            Log.AcquiringTokenFailed(_logger, e);

            return e is AzureServiceTokenProviderException // BadRequest
                ? RetryParams.StopRetrying
                : RetryParams.DefaultBackOff(retryCount);
        }

        /// <summary>
        /// Log messages for <see cref="ManagedIdentityAuthenticator"/>.
        /// </summary>
        /// <remarks>
        /// Messages implemented using <see cref="LoggerMessage.Define(LogLevel, EventId, string)"/> to maximize performance.
        /// For more information, see https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/loggermessage?view=aspnetcore-5.0.
        /// </remarks>
        private static class Log
        {
            private static readonly Action<ILogger, string, long, Exception> _acquiredToken =
                LoggerMessage.Define<string, long>(LogLevel.Information, new EventId(1, nameof(AcquiredToken)), "{String}: Acquired token using MSI in {Int64}.");

            private static readonly Action<ILogger, Exception> _acquiringTokenFailed =
                LoggerMessage.Define(LogLevel.Error, new EventId(2, nameof(AcquiringTokenFailed)), "Exception when trying to acquire token using MSI!.");

            public static void AcquiredToken(ILogger logger, string name, long elapsedMilliseconds) => _acquiredToken(logger, name, elapsedMilliseconds, null);

            public static void AcquiringTokenFailed(ILogger logger, Exception ex) => _acquiringTokenFailed(logger, ex);
        }
    }
}
