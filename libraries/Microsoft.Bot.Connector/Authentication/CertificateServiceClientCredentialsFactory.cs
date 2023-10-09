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
        private readonly X509Certificate2 _certificate;
        private readonly bool _sendX5c = false;
        private readonly string _appId;
        private readonly string _tenantId;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateServiceClientCredentialsFactory"/> class.
        /// </summary>
        /// <param name="certificate">The certificate to use for authentication.</param>
        /// <param name="appId">Microsoft application Id related to the certificate.</param>
        /// <param name="tenantId">The oauth token tenant.</param>
        /// <param name="httpClient">A custom httpClient to use.</param>
        /// <param name="logger">A logger instance to use.</param>
        public CertificateServiceClientCredentialsFactory(X509Certificate2 certificate, string appId, string tenantId = null, HttpClient httpClient = null, ILogger logger = null)
            : base()
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }

            _certificate = certificate ?? throw new ArgumentNullException(nameof(certificate));
            _appId = appId;
            _tenantId = tenantId;
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateServiceClientCredentialsFactory"/> class.
        /// </summary>
        /// <param name="certificate">The certificate to use for authentication.</param>
        /// <param name="sendX5c">If true will send the public certificate to Azure AD along with the token request, so that
        /// Azure AD can use it to validate the subject name based on a trusted issuer policy.</param>
        /// <param name="appId">Microsoft application Id related to the certificate.</param>
        /// <param name="tenantId">The oauth token tenant.</param>
        /// <param name="httpClient">A custom httpClient to use.</param>
        /// <param name="logger">A logger instance to use.</param>
        public CertificateServiceClientCredentialsFactory(X509Certificate2 certificate, bool sendX5c, string appId, string tenantId = null, HttpClient httpClient = null, ILogger logger = null)
            : this(certificate, appId, tenantId, httpClient, logger)
        {
            _sendX5c = sendX5c;
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

            return Task.FromResult<ServiceClientCredentials>(
                new CertificateAppCredentials(_certificate, _sendX5c, _appId, _tenantId, _httpClient, _logger));
        }
    }
}
