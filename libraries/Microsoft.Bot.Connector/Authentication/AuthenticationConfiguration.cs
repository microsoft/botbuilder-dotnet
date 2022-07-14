// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// General configuration settings for authentication.
    /// </summary>
    /// <remarks>
    /// Note that this is explicitly a class and not an interface,
    /// since interfaces don't support default values, after the initial release any change would break backwards compatibility.
    /// </remarks>
    public class AuthenticationConfiguration
    {
        /// <summary>
        /// Gets or sets an array of JWT endorsements.
        /// </summary>
        /// <value>
        /// An array of JWT endorsements.
        /// </value>
#pragma warning disable CA1819 // Properties should not return arrays (we can't change this without breaking binary compat)
        public string[] RequiredEndorsements { get; set; } = Array.Empty<string>();
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary>
        /// Gets or sets an <see cref="ClaimsValidator"/> instance used to validate the identity claims.
        /// </summary>
        /// <value>
        /// An <see cref="ClaimsValidator"/> instance used to validate the identity claims.
        /// </value>
        public virtual ClaimsValidator ClaimsValidator { get; set; } = null;

        /// <summary>
        /// Gets or sets a collection of valid JWT token issuers.
        /// </summary>
        /// <value>
        /// A collection of valid JWT token issuers.
        /// </value>
        public IEnumerable<string> ValidTokenIssuers { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether
        /// the x5c claim (public key of the certificate) should be sent to
        /// the STS. Sending the x5c enables application developers to achieve easy certificate
        /// roll-over in Azure AD: this method will send the public certificate to Azure
        /// AD along with the token request, so that Azure AD can use it to validate the
        /// subject name based on a trusted issuer policy. This saves the application admin
        /// from the need to explicitly manage the certificate rollover (either via portal
        /// or PowerShell/CLI operation). For details see https://aka.ms/msal-net-sni.
        ///
        /// <example>
        /// Register this in startup.cs of your application along with <see cref="MsalServiceClientCredentialsFactory"/>>.
        /// <see cref="MsalAppCredentials"/> for more infromation.
        /// <code>
        ///  // MSAL certificate authentication
        ///  services.AddTransient&lt;IConfidentialClientApplication&gt;(
        ///  serviceProvider => ConfidentialClientApplicationBuilder.Create(Configuration.GetSection("MicrosoftAppId").Value)
        ///      .WithCertificate(new X509Certificate2("pathToCertFile", "certPassword"))
        ///      .Build());
        ///      
        ///  // Configure Authentication configuration.
        ///  services.AddSingleton(sp => new AuthenticationConfiguration { SendX5c = true });
        ///  
        ///  // MSAL credential factory: regardless of secret, cert or custom auth, need to add the line below
        ///  services.AddTransient&lt;ServiceClientCredentialsFactory, Authentication.MsalServiceClientCredentialsFactory&gt;();
        ///  
        ///  // Create the Bot Framework Authentication to be used with the Bot Adapter.
        ///  services.AddSingleton&lt;BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication&gt;();
        /// </code>
        /// </example>
        /// </summary>
        /// <value>
        /// true if the x5c should be sent. Otherwise false. The default is false.
        /// </value>
        public bool SendX5c { get; set; } = false;
    }
}
