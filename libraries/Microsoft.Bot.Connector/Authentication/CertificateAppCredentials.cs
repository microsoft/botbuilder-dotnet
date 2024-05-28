// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// CertificateAppCredentials auth implementation and cache.
    /// </summary>
    public class CertificateAppCredentials : AppCredentials
    {
        private readonly X509Certificate2 clientCertificate;
        private readonly bool sendX5c;

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateAppCredentials"/> class.
        /// </summary>
        /// <param name="options">Options for this CertificateAppCredentials.</param>
        public CertificateAppCredentials(CertificateAppCredentialsOptions options)
             : base(options.ChannelAuthTenant, options.CustomHttpClient, options.Logger, oAuthScope: options.OauthScope)
        {
            if (options.ClientCertificate == null)
            {
                throw new ArgumentNullException(nameof(options), "ClientCertificate is required.");
            }

            if (string.IsNullOrEmpty(options.AppId))
            {
                throw new ArgumentNullException(nameof(options), "AppId is required.");
            }

            sendX5c = options.SendX5c;
            clientCertificate = options.ClientCertificate;
            MicrosoftAppId = options.AppId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateAppCredentials"/> class.
        /// </summary>
        /// <param name="clientCertificate">Client certificate to be presented for authentication.</param>
        /// <param name="appId">Microsoft application Id related to the certifiacte.</param>
        /// <param name="channelAuthTenant">Optional. The oauth token tenant.</param>
        /// <param name="customHttpClient">Optional <see cref="HttpClient"/> to be used when acquiring tokens.</param>
        /// <param name="logger">Optional <see cref="ILogger"/> to gather telemetry data while acquiring and managing credentials.</param>
        public CertificateAppCredentials(X509Certificate2 clientCertificate, string appId, string channelAuthTenant = null, HttpClient customHttpClient = null, ILogger logger = null)
            : this(clientCertificate, appId, channelAuthTenant, string.Empty, false, customHttpClient, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateAppCredentials"/> class.
        /// </summary>
        /// <param name="clientCertificate">Client certificate to be presented for authentication.</param>
        /// <param name="sendX5c">This parameter, if true, enables application developers to achieve easy certificates roll-over in Azure AD: setting this parameter to true will send the public certificate to Azure AD along with the token request, so that Azure AD can use it to validate the subject name based on a trusted issuer policy. </param>
        /// <param name="appId">Microsoft application Id related to the certifiacte.</param>
        /// <param name="channelAuthTenant">Optional. The oauth token tenant.</param>
        /// <param name="customHttpClient">Optional <see cref="HttpClient"/> to be used when acquiring tokens.</param>
        /// <param name="logger">Optional <see cref="ILogger"/> to gather telemetry data while acquiring and managing credentials.</param>
        public CertificateAppCredentials(X509Certificate2 clientCertificate, bool sendX5c, string appId, string channelAuthTenant = null, HttpClient customHttpClient = null, ILogger logger = null)
            : this(clientCertificate, appId, channelAuthTenant, string.Empty, sendX5c, customHttpClient, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateAppCredentials"/> class.
        /// </summary>
        /// <param name="clientCertificate">Client certificate to be presented for authentication.</param>
        /// <param name="appId">Microsoft application Id related to the certifiacte.</param>
        /// <param name="channelAuthTenant">Optional. The oauth token tenant.</param>
        /// <param name="oAuthScope">Optional. The scope for the token.</param>
        /// <param name="sendX5c">Optional. This parameter, if true, enables application developers to achieve easy certificates roll-over in Azure AD: setting this parameter to true will send the public certificate to Azure AD along with the token request, so that Azure AD can use it to validate the subject name based on a trusted issuer policy. </param>
        /// <param name="customHttpClient">Optional <see cref="HttpClient"/> to be used when acquiring tokens.</param>
        /// <param name="logger">Optional <see cref="ILogger"/> to gather telemetry data while acquiring and managing credentials.</param>
        public CertificateAppCredentials(X509Certificate2 clientCertificate, string appId, string channelAuthTenant = null, string oAuthScope = null, bool sendX5c = false, HttpClient customHttpClient = null, ILogger logger = null)
            : base(channelAuthTenant, customHttpClient, logger, oAuthScope: oAuthScope)
        {
            if (clientCertificate == null)
            {
                throw new ArgumentNullException(nameof(clientCertificate));
            }

            if (string.IsNullOrEmpty(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }

            this.sendX5c = sendX5c;
            this.clientCertificate = clientCertificate;
            MicrosoftAppId = appId;
        }

        /// <inheritdoc/>
        protected override Lazy<IAuthenticator> BuildIAuthenticator()
        {
            return new Lazy<IAuthenticator>(
                () =>
                {
                    var clientApplication = CreateClientApplication(clientCertificate, MicrosoftAppId, sendX5c, CustomHttpClient);
                    return new MsalAppCredentials(
                        clientApplication,
                        MicrosoftAppId,
                        OAuthEndpoint,
                        OAuthScope,
                        ValidateAuthority,
                        Logger);
                },
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        private Identity.Client.IConfidentialClientApplication CreateClientApplication(X509Certificate2 clientCertificate, string appId, bool sendX5c, HttpClient customHttpClient = null)
        {
            var clientBuilder = Identity.Client.ConfidentialClientApplicationBuilder.Create(appId)
               .WithAuthority(new Uri(OAuthEndpoint), ValidateAuthority)
               .WithCertificate(clientCertificate, sendX5c);

            if (customHttpClient != null)
            {
                clientBuilder.WithHttpClientFactory(new ConstantHttpClientFactory(customHttpClient));
            }

            return clientBuilder.Build();
        }
    }
}
