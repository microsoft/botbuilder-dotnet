// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// CertificateAppCredentials Options.
    /// </summary>
    public class CertificateAppCredentialsOptions
    {
        /// <summary>
        /// Gets or sets the X509Certificate2 ClientCertificate.
        /// </summary>
        /// <value>
        /// ClientCertificate.
        /// </value>
        public X509Certificate2 ClientCertificate { get; set; }

        /// <summary>
        /// Gets or sets the AppId.
        /// </summary>
        /// <value>
        /// AppId.
        /// </value>
        public string AppId { get; set; }

        /// <summary>
        /// Gets or sets the Channel Auth Tenant.
        /// </summary>
        /// <value>
        /// ChannelAuthTenant.
        /// </value>
        public string ChannelAuthTenant { get; set; } = null;

        /// <summary>
        /// Gets or sets the OauthScope.
        /// </summary>
        /// <value>
        /// OauthScope.
        /// </value>
        public string OauthScope { get; set; } = null;

        /// <summary>
        /// Gets or sets the CustomHttpClient.
        /// </summary>
        /// <value>
        /// CustomHttpClient.
        /// </value>
        public HttpClient CustomHttpClient { get; set; } = null;

        /// <summary>
        /// Gets or sets the Logger.
        /// </summary>
        /// <value>
        /// Logger.
        /// </value>
        public ILogger Logger { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether this parameter, if true,
        /// enables application developers to achieve easy certificates roll-over
        /// in Azure AD: setting this parameter to true will send the public 
        /// certificate to Azure AD along with the token request, so that
        /// Azure AD can use it to validate the subject name based on a trusted issuer policy.
        /// </summary>
        /// <value>
        /// SendX5c if true.
        /// </value>
        public bool SendX5c { get; set; }
    }
}
