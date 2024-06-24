// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// CertificateGovAppCredentials auth implementation for Gov Cloud.
    /// </summary>
    public class CertificateGovernmentAppCredentials : CertificateAppCredentials
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateGovernmentAppCredentials"/> class.
        /// </summary>
        /// <param name="options">Options for this CertificateAppCredentials.</param>
        public CertificateGovernmentAppCredentials(CertificateAppCredentialsOptions options)
            : base(options)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateGovernmentAppCredentials"/> class.
        /// </summary>
        /// <param name="clientCertificate">Client certificate to be presented for authentication.</param>
        /// <param name="appId">Microsoft application Id related to the certificate.</param>
        /// <param name="channelAuthTenant">Optional. The oauth token tenant.</param>
        /// <param name="customHttpClient">Optional <see cref="HttpClient"/> to be used when acquiring tokens.</param>
        /// <param name="logger">Optional <see cref="ILogger"/> to gather telemetry data while acquiring and managing credentials.</param>
        public CertificateGovernmentAppCredentials(X509Certificate2 clientCertificate, string appId, string channelAuthTenant = null, HttpClient customHttpClient = null, ILogger logger = null)
            : base(clientCertificate, appId, channelAuthTenant, customHttpClient, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateGovernmentAppCredentials"/> class.
        /// </summary>
        /// <param name="clientCertificate">Client certificate to be presented for authentication.</param>
        /// <param name="sendX5c">This parameter, if true, enables application developers to achieve easy certificates roll-over in Azure AD: setting this parameter to true will send the public certificate to Azure AD along with the token request, so that Azure AD can use it to validate the subject name based on a trusted issuer policy. </param>
        /// <param name="appId">Microsoft application Id related to the certificate.</param>
        /// <param name="channelAuthTenant">Optional. The oauth token tenant.</param>
        /// <param name="customHttpClient">Optional <see cref="HttpClient"/> to be used when acquiring tokens.</param>
        /// <param name="logger">Optional <see cref="ILogger"/> to gather telemetry data while acquiring and managing credentials.</param>
        public CertificateGovernmentAppCredentials(X509Certificate2 clientCertificate, bool sendX5c, string appId, string channelAuthTenant = null, HttpClient customHttpClient = null, ILogger logger = null)
            : base(clientCertificate, sendX5c, appId, channelAuthTenant, customHttpClient, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateGovernmentAppCredentials"/> class.
        /// </summary>
        /// <param name="clientCertificate">Client certificate to be presented for authentication.</param>
        /// <param name="appId">Microsoft application Id related to the certificate.</param>
        /// <param name="channelAuthTenant">Optional. The oauth token tenant.</param>
        /// <param name="oAuthScope">Optional. The scope for the token.</param>
        /// <param name="sendX5c">Optional. This parameter, if true, enables application developers to achieve easy certificates roll-over in Azure AD: setting this parameter to true will send the public certificate to Azure AD along with the token request, so that Azure AD can use it to validate the subject name based on a trusted issuer policy. </param>
        /// <param name="customHttpClient">Optional <see cref="HttpClient"/> to be used when acquiring tokens.</param>
        /// <param name="logger">Optional <see cref="ILogger"/> to gather telemetry data while acquiring and managing credentials.</param>
        public CertificateGovernmentAppCredentials(X509Certificate2 clientCertificate, string appId, string channelAuthTenant = null, string oAuthScope = null, bool sendX5c = false, HttpClient customHttpClient = null, ILogger logger = null)
            : base(clientCertificate, appId, channelAuthTenant, oAuthScope, sendX5c, customHttpClient, logger)
        {
        }

        /// <inheritdoc/>
        protected override string DefaultChannelAuthTenant => GovernmentAuthenticationConstants.DefaultChannelAuthTenant;

        /// <inheritdoc/>
        protected override string ToChannelFromBotOAuthScope => GovernmentAuthenticationConstants.ToChannelFromBotOAuthScope;

        /// <inheritdoc/>
        protected override string ToChannelFromBotLoginUrlTemplate => GovernmentAuthenticationConstants.ToChannelFromBotLoginUrlTemplate;
    }
}
