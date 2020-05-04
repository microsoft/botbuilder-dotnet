// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Microsoft.Bot.Connector.Authentication
{
    public class AdalAuthenticator : IAuthenticator
    {
        private const string MsalTemporarilyUnavailable = "temporarily_unavailable";

        // When TargetFramework moves to netstandard2.1, use HttpStatusCode.TooManyRequests
        // https://docs.microsoft.com/en-us/dotnet/api/system.net.httpstatuscode?view=netstandard-2.1#System_Net_HttpStatusCode_TooManyRequests
        private const int HttpTooManyRequests = 429;

        // Semaphore to control concurrency while refreshing tokens from ADAL.
        // Whenever a token expires, we want only one request to retrieve a token.
        // Cached requests take less than 0.1 millisecond to resolve, so the semaphore doesn't hurt performance under load tests
        // unless we have more than 10,000 requests per second, but in that case other things would break first.
        private static SemaphoreSlim tokenRefreshSemaphore = new SemaphoreSlim(1, 1);
        private static readonly TimeSpan SemaphoreTimeout = TimeSpan.FromSeconds(10);

        // Depending on the responses we get from the service, we update a shared retry policy with the RetryAfter header
        // from the HTTP 429 we receive.
        // When everything seems to be OK, this retry policy will be empty.
        // The reason for this is that if a request gets throttled, even if we wait to retry that, another thread will try again right away.
        // With the shared retry policy, if a request gets throttled, we know that other threads have to wait as well.
        // This variable is guarded by the authContextSemaphore semaphore. Don't modify it outside of the semaphore scope.
        private static volatile RetryParams currentRetryPolicy;

        // Our ADAL context. Acquires tokens and manages token caching for us.
        private AuthenticationContext authContext;

        private readonly ClientCredential clientCredential;
        private readonly ClientAssertionCertificate clientCertificate;
        private readonly bool clientCertSendX5c;
        private readonly OAuthConfiguration authConfig;
        private readonly ILogger logger;

        public AdalAuthenticator(ClientCredential clientCredential, OAuthConfiguration configurationOAuth, HttpClient customHttpClient = null)
            : this(clientCredential, configurationOAuth, customHttpClient, null)
        {
        }

        public AdalAuthenticator(ClientCredential clientCredential, OAuthConfiguration configurationOAuth, HttpClient customHttpClient = null, ILogger logger = null)
        {
            this.authConfig = configurationOAuth ?? throw new ArgumentNullException(nameof(configurationOAuth));
            this.clientCredential = clientCredential ?? throw new ArgumentNullException(nameof(clientCredential));
            this.logger = logger;

            Initialize(configurationOAuth, customHttpClient);
        }

        public AdalAuthenticator(ClientAssertionCertificate clientCertificate, OAuthConfiguration configurationOAuth, HttpClient customHttpClient = null, ILogger logger = null)
            : this(clientCertificate, false, configurationOAuth, customHttpClient, logger)
        {
        }

        public AdalAuthenticator(ClientAssertionCertificate clientCertificate, bool sendX5c, OAuthConfiguration configurationOAuth, HttpClient customHttpClient = null, ILogger logger = null)
        {
            this.authConfig = configurationOAuth ?? throw new ArgumentNullException(nameof(configurationOAuth));
            this.clientCertificate = clientCertificate ?? throw new ArgumentNullException(nameof(clientCertificate));
            this.logger = logger;
            this.clientCertSendX5c = sendX5c;

            Initialize(configurationOAuth, customHttpClient);
        }

        public async Task<AuthenticationResult> GetTokenAsync(bool forceRefresh = false)
        {
            var watch = Stopwatch.StartNew();

            var result = await Retry.Run(
                task: () => AcquireTokenAsync(forceRefresh),
                retryExceptionHandler: (ex, ct) => HandleAdalException(ex, ct)).ConfigureAwait(false);

            watch.Stop();
            logger?.LogInformation($"GetTokenAsync: Acquired token using ADAL in {watch.ElapsedMilliseconds}.");

            return result;
        }

        async Task<AuthenticatorResult> IAuthenticator.GetTokenAsync(bool forceRefresh)
        {
            var result = await GetTokenAsync(forceRefresh);
            return new AuthenticatorResult() 
            {
                AccessToken = result.AccessToken,
                ExpiresOn = result.ExpiresOn
            };
        }

        private async Task<AuthenticationResult> AcquireTokenAsync(bool forceRefresh = false)
        {
            bool acquired = false;

            if (forceRefresh)
            {
                authContext.TokenCache.Clear();
            }

            try
            {
                // The ADAL client team recommends limiting concurrency of calls. When the Token is in cache there is never
                // contention on this semaphore, but when tokens expire there is some. However, after measuring performance
                // with and without the semaphore (and different configs for the semaphore), not limiting concurrency actually
                // results in higher response times overall. Without the use of this semaphore calls to AcquireTokenAsync can take up
                // to 5 seconds under high concurrency scenarios.
                acquired = tokenRefreshSemaphore.Wait(SemaphoreTimeout);

                // If we are allowed to enter the semaphore, acquire the token.
                if (acquired)
                {
                    // Acquire token async using MSAL.NET
                    // https://github.com/AzureAD/azure-activedirectory-library-for-dotnet
                    // Given that this is a ClientCredential scenario, it will use the cache without the
                    // need to call AcquireTokenSilentAsync (which is only for user credentials).
                    // Scenario details: https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Client-credential-flows#it-uses-the-application-token-cache
                    AuthenticationResult authResult = null;

                    // Password based auth
                    if (clientCredential != null)
                    {
                        authResult = await authContext.AcquireTokenAsync(authConfig.Scope, this.clientCredential).ConfigureAwait(false);
                    }

                    // Certificate based auth
                    else if (clientCertificate != null)
                    {
                        authResult = await authContext.AcquireTokenAsync(authConfig.Scope, clientCertificate, sendX5c: this.clientCertSendX5c).ConfigureAwait(false);
                    }

                    // This means we acquired a valid token successfully. We can make our retry policy null.
                    // Note that the retry policy is set under the semaphore so no additional synchronization is needed.
                    if (currentRetryPolicy != null)
                    {
                        currentRetryPolicy = null;
                    }

                    return authResult;
                }
                else
                {
                    // If the token is taken, it means that one thread is trying to acquire a token from the server.
                    // If we already received information about how much to throttle, it will be in the currentRetryPolicy.
                    // Use that to inform our next delay before trying.
                    throw new ThrottleException() { RetryParams = currentRetryPolicy };
                }
            }
            catch (Exception ex)
            {
                // If we are getting throttled, we set the retry policy according to the RetryAfter headers
                // that we receive from the auth server.
                // Note that the retry policy is set under the semaphore so no additional synchronization is needed.
                if (IsAdalServiceUnavailable(ex))
                {
                    currentRetryPolicy = ComputeAdalRetry(ex);
                }

                throw;
            }
            finally
            {
                // Always release the semaphore if we acquired it.
                if (acquired)
                {
                    ReleaseSemaphore();
                }
            }
        }

        private void ReleaseSemaphore()
        {
            try
            {
                tokenRefreshSemaphore.Release();
            }
            catch (SemaphoreFullException)
            {
                // We should not be hitting this after switching to SemaphoreSlim, but if we do hit it everything will keep working.
                // Logging to have clear knowledge of whether this is happening.
                logger?.LogWarning("Attempted to release a full semaphore.");
            }

            // Any exception other than SemaphoreFullException should be thrown right away
        }

        private RetryParams HandleAdalException(Exception ex, int currentRetryCount)
        {
            if (IsAdalServiceUnavailable(ex))
            {
                return ComputeAdalRetry(ex);
            }
            else if (ex is ThrottleException)
            {
                // This is an exception that we threw, with knowledge that
                // one of our threads is trying to acquire a token from the server
                // Use the retry parameters recommended in the exception
                ThrottleException throttlException = (ThrottleException)ex;
                return throttlException.RetryParams ?? RetryParams.DefaultBackOff(currentRetryCount);
            }
            else if (IsAdalServiceInvalidRequest(ex))
            {
                return RetryParams.StopRetrying;
            }
            else
            {
                // We end up here is the exception is not an ADAL exception. An example, is under high traffic
                // where we could have a timeout waiting to acquire a token, waiting on the semaphore.
                // If we hit a timeout, we want to retry a reasonable number of times.
                return RetryParams.DefaultBackOff(currentRetryCount);
            }
        }

        private bool IsAdalServiceUnavailable(Exception ex)
        {
            AdalServiceException adalServiceException = ex as AdalServiceException;
            if (adalServiceException == null)
            {
                return false;
            }

            // When the Service Token Server (STS) is too busy because of “too many requests”,
            // it returns an HTTP error 429
            return adalServiceException.ErrorCode == MsalTemporarilyUnavailable || adalServiceException.StatusCode == HttpTooManyRequests;
        }

        /// <summary>
        /// Determine whether exception represents an invalid request from AAD.
        /// </summary>
        /// <param name="ex">Exception.</param>
        /// <returns>True if the exception represents an invalid request.</returns>
        private bool IsAdalServiceInvalidRequest(Exception ex)
        {
            if (ex is AdalServiceException adal)
            {
                // ErrorCode is "invalid_request"
                // but HTTP StatusCode covers more non-retryable conditions
                if (adal.StatusCode == (int)HttpStatusCode.BadRequest)
                {
                    return true;
                }
            }

            return false;
        }

        private RetryParams ComputeAdalRetry(Exception ex)
        {
            if (ex is AdalServiceException)
            {
                AdalServiceException adalServiceException = (AdalServiceException)ex;

                // When the Service Token Server (STS) is too busy because of “too many requests”,
                // it returns an HTTP error 429 with a hint about when you can try again (Retry-After response field) as a delay in seconds
                if (adalServiceException.ErrorCode == MsalTemporarilyUnavailable || adalServiceException.StatusCode == HttpTooManyRequests)
                {
                    RetryConditionHeaderValue retryAfter = adalServiceException.Headers.RetryAfter;

                    // Depending on the service, the recommended retry time may be in retryAfter.Delta or retryAfter.Date. Check both.
                    if (retryAfter != null && retryAfter.Delta.HasValue)
                    {
                        return new RetryParams(retryAfter.Delta.Value);
                    }
                    else if (retryAfter != null && retryAfter.Date.HasValue)
                    {
                        return new RetryParams(retryAfter.Date.Value.Offset);
                    }

                    // We got a 429 but didn't get a specific back-off time. Use the default
                    return RetryParams.DefaultBackOff(0);
                }
            }

            return RetryParams.DefaultBackOff(0);
        }

        private void Initialize(OAuthConfiguration configurationOAuth, HttpClient customHttpClient)
        {
            if (customHttpClient != null)
            {
                var httpClientFactory = new ConstantHttpClientFactory(customHttpClient);
                this.authContext = new AuthenticationContext(configurationOAuth.Authority, true, new TokenCache(), httpClientFactory);
            }
            else
            {
                this.authContext = new AuthenticationContext(configurationOAuth.Authority);
            }
        }

        private bool UseCertificate()
        {
            return this.clientCertificate != null;
        }
    }
}
