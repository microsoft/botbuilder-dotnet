// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// An authentication class that implements <see cref="IAuthenticator"/>, used to acquire tokens for outgoing messages to the channels.
    /// </summary>
    public class MsalAppCredentials : AppCredentials, IAuthenticator
    {
        /// <summary>
        /// An empty set of credentials.
        /// </summary>
        public static readonly MsalAppCredentials Empty = new MsalAppCredentials(clientApplication: null, appId: null);

        // Semaphore to control concurrency while refreshing tokens from MSAL.
        // Whenever a token expires, we want only one request to retrieve a token.
        // Cached requests take less than 0.1 millisecond to resolve, so the semaphore doesn't hurt performance under load tests
        // unless we have more than 10,000 requests per second, but in that case other things would break first.
        private static SemaphoreSlim tokenRefreshSemaphore = new SemaphoreSlim(1, 1);
        private static readonly TimeSpan SemaphoreTimeout = TimeSpan.FromSeconds(10);

        // Our MSAL application. Acquires tokens and manages token caching for us.
        private readonly IConfidentialClientApplication _clientApplication;

        private readonly ILogger _logger;
        private readonly string _scope;
        private readonly string _authority;
        private readonly bool _validateAuthority;

        /// <summary>
        /// Initializes a new instance of the <see cref="MsalAppCredentials"/> class.
        /// </summary>
        /// <param name="clientApplication">The client application to use to acquire tokens.</param>
        /// <param name="appId">The Microsoft application Id.</param>
        /// <param name="logger">Optional <see cref="ILogger"/>.</param>
        /// <param name="authority">Optional authority.</param>
        /// <param name="validateAuthority">Whether to validate the authority.</param>
        /// <param name="scope">Optional custom scope.</param>
        public MsalAppCredentials(IConfidentialClientApplication clientApplication, string appId, string authority = null, string scope = null, bool validateAuthority = true, ILogger logger = null)
            : base(null, null, logger, scope)
        {
            MicrosoftAppId = appId;
            _clientApplication = clientApplication;
            _logger = logger;
            _scope = scope;
            _authority = authority;
            _validateAuthority = validateAuthority;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MsalAppCredentials"/> class.
        /// </summary>
        /// <param name="appId">The Microsoft application id.</param>
        /// <param name="appPassword">The Microsoft application password.</param>
        /// <param name="authority">Optional authority.</param>
        /// <param name="validateAuthority">Whether to validate the authority.</param>
        /// <param name="scope">Optional custom scope.</param>
        /// <param name="logger">Optional <see cref="ILogger"/>.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2234:Pass system uri objects instead of strings", Justification = "Using string overload for legacy compatibility.")]
        public MsalAppCredentials(string appId, string appPassword, string authority = null, string scope = null, bool validateAuthority = true, ILogger logger = null)
            : this(
                  clientApplication: ConfidentialClientApplicationBuilder.Create(appId).WithClientSecret(appPassword).Build(),
                  appId: appId,
                  authority: authority,
                  scope: scope,
                  validateAuthority: validateAuthority,
                  logger: logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MsalAppCredentials"/> class.
        /// </summary>
        /// <param name="appId">The Microsoft application id.</param>
        /// <param name="certificate">The certificate to use for authentication.</param>
        /// <param name="validateAuthority">Optional switch for whether to validate the authority.</param>
        /// <param name="authority">Optional authority.</param>
        /// <param name="scope">Optional custom scope.</param>
        /// <param name="logger">Optional <see cref="ILogger"/>.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2234:Pass system uri objects instead of strings", Justification = "Using string overload for legacy compatibility.")]
        public MsalAppCredentials(string appId, X509Certificate2 certificate, string authority = null, string scope = null, bool validateAuthority = true, ILogger logger = null)
            : this(
                  clientApplication: ConfidentialClientApplicationBuilder.Create(appId).WithCertificate(certificate).Build(),
                  appId: appId,
                  authority: authority,
                  scope: scope,
                  validateAuthority: validateAuthority,
                  logger: logger)
        {
        }

        async Task<AuthenticatorResult> IAuthenticator.GetTokenAsync(bool forceRefresh)
        {
            var watch = Stopwatch.StartNew();

            var result = await Retry.Run(
                task: () => AcquireTokenAsync(forceRefresh),
                retryExceptionHandler: (ex, ct) => HandleMsalException(ex, ct)).ConfigureAwait(false);

            watch.Stop();
            _logger?.LogInformation($"GetTokenAsync: Acquired token using ADAL in {watch.ElapsedMilliseconds}.");

            return result;
        }

        /// <inheritdoc/>
        protected override Lazy<AdalAuthenticator> BuildAuthenticator()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override Lazy<IAuthenticator> BuildIAuthenticator()
        {
            return new Lazy<IAuthenticator>(() => this, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        private async Task<AuthenticatorResult> AcquireTokenAsync(bool forceRefresh = false)
        {
            if (_clientApplication == null)
            {
                throw new InvalidOperationException("AcquireTokenAsync should not be called for empty credentials.");
            }

            bool acquired = false;

            try
            {
                // Limiting concurrency on MSAL token acquisitions. When the Token is in cache there is never
                // contention on this semaphore, but when tokens expire there is some. However, after measuring performance
                // with and without the semaphore (and different configs for the semaphore), not limiting concurrency actually
                // results in higher response times, more throttling and more contention. 
                // Without the use of this semaphore calls to AcquireTokenAsync can take tens of seconds under high concurrency scenarios.
#pragma warning disable VSTHRD103 // Call async methods when in an async method
                acquired = tokenRefreshSemaphore.Wait(SemaphoreTimeout);
#pragma warning restore VSTHRD103 // Call async methods when in an async method

                // If we are allowed to enter the semaphore, acquire the token.
                if (acquired)
                {
                    // Note that in MSAL, we dont pass resources anymore, and we instead pass scopes. To be recognized by MSAL, we append the '/.default' to the scope.
                    // Scope requirements described in MSAL migration spec: https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-net-migration.
                    const string scopePostFix = "/.default";
                    var scope = _scope ?? OAuthScope;

                    if (!scope.EndsWith(scopePostFix, StringComparison.OrdinalIgnoreCase))
                    {
                        scope = $"{scope}{scopePostFix}";
                    }

                    // Acquire token async using MSAL.NET
                    // This will use the cache from the application cache of the MSAL library, no external caching is needed.
                    var msalResult = await _clientApplication
                        .AcquireTokenForClient(new[] { scope })
                        .WithAuthority(_authority ?? OAuthEndpoint, _validateAuthority)
                        .WithForceRefresh(forceRefresh)
                        .ExecuteAsync().ConfigureAwait(false);

                    // This means we acquired a valid token successfully. We can make our retry policy null.
                    return new AuthenticatorResult()
                    { 
                        AccessToken = msalResult.AccessToken,
                        ExpiresOn = msalResult.ExpiresOn
                    };
                }
                else
                {
                    // If the token is taken, it means that one thread is trying to acquire a token from the server.
                    // Throttle this request to allow the currently running request to fulfill and then let this one get the result from the cache.
                    throw new ThrottleException() { RetryParams = RetryParams.DefaultBackOff(0) };
                }
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
                // This should never happen but we want to know if it does.
                _logger?.LogWarning("Attempted to release a full semaphore.");
            }

            // Any exception other than SemaphoreFullException should be thrown right away
        }

        private RetryParams HandleMsalException(Exception ex, int ct)
        {
            _logger?.LogError(ex, "Exception acquiring token through MSAL.");

            if (ex is MsalServiceException msalException)
            {
                _logger?.LogWarning(msalException, $"MSAL service error code: {msalException.ErrorCode}.");

                // Service error with status code "temporarily_unavailable" is retryable.
                // Spec and reference: https://docs.microsoft.com/en-us/azure/active-directory/develop/reference-aadsts-error-codes.
                if (msalException.ErrorCode == "temporarily_unavailable")
                {
                    return RetryParams.DefaultBackOff(ct);
                }
            }

            return RetryParams.StopRetrying;
        }
    }
}
