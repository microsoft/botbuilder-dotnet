// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// A Managed Identity implementation of the <see cref="ServiceClientCredentialsFactory"/> interface.
    /// </summary>
    public class CertificateServiceClientCredentialsFactory : ServiceClientCredentialsFactory
    {
        private readonly CertificateAppCredentials _certificateAppCredentials;
        private readonly string _appId;

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateServiceClientCredentialsFactory"/> class.
        /// </summary>
        /// <param name="certificate">The certificate to use for authentication.</param>
        /// <param name="appId">Microsoft application Id related to the certificate.</param>
        /// <param name="tenantId">The oauth token tenant.</param>
        /// <param name="httpClient">A custom httpClient to use.</param>
        /// <param name="logger">A logger instance to use.</param>
        /// <param name="sendX5c">A flag if CertificateAppCredentials should send certificate chains in the request.
        /// It enables authentication with AAD using certificate subject name (not CNAME) and issuer instead of a thumbprint.
        /// </param>
        public CertificateServiceClientCredentialsFactory(
            X509Certificate2 certificate,
            string appId,
            string tenantId = null,
            HttpClient httpClient = null,
            ILogger logger = null,
            bool sendX5c = false)
            : base()
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }

            _appId = appId;

            // Instance must be reused otherwise it will cause throttling on AAD.
            _certificateAppCredentials = new CertificateAppCredentials(
                certificate ?? throw new ArgumentNullException(nameof(certificate)),
                sendX5c,
                appId,
                tenantId,
                httpClient,
                logger);
        }

        /// <inheritdoc />
        public override Task<bool> IsValidAppIdAsync(string appId, CancellationToken cancellationToken)
        {
            return Task.FromResult(appId == _appId);
        }

        /// <inheritdoc />
        public override Task<bool> IsAuthenticationDisabledAsync(CancellationToken cancellationToken)
        {
            // Auth is always enabled for Certificate.
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        public override Task<ServiceClientCredentials> CreateCredentialsAsync(
            string appId, string audience, string loginEndpoint, bool validateAuthority, CancellationToken cancellationToken)
        {
            if (appId != _appId)
            {
                throw new InvalidOperationException("Invalid Managed ID.");
            }

            return Task.FromResult<ServiceClientCredentials>(_certificateAppCredentials);
        }
    }
}
